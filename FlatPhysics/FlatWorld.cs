using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace FlatPhysics
{
    public sealed class FlatWorld
    {
        public static readonly float MinBodySize = 0.01f * 0.01f;
        public static readonly float MaxBodySize = 64f * 64f;
        public static readonly float MinDensity = 0.5f;         // g/cm^3
        public static readonly float MaxDensity = 21.4f;

        private static readonly int MinIteration = 5;
        private static readonly int MaxIteration = 128;

        private FlatVector gravity;
        private List<FlatBody> bodyList;
        private List<(int, int)> contactPairs;

        FlatVector[] contactList;
        FlatVector[] impulseList;
        FlatVector[] raList;
        FlatVector[] rbList;
        FlatVector[] frictionImpulseList;
        float[] jList;



        public FlatWorld()
        {
            this.gravity = new FlatVector(0f, -9.81f);
            this.bodyList = new List<FlatBody>();
            this.contactPairs = new List<(int, int)>();

            this.contactList = new FlatVector[2];
            this.impulseList = new FlatVector[2];
            this.raList = new FlatVector[2];
            this.rbList = new FlatVector[2];
            this.frictionImpulseList = new FlatVector[2];
            this.jList = new float[2];
        }

        public int BodyCount
        {
            get { return this.bodyList.Count; }
        }

        public void AddBody(FlatBody body)
        {
            this.bodyList.Add(body);
        }

        public void RemoveBody(FlatBody body)
        {
            this.bodyList.Remove(body);
        }

        public bool GetBody(int index, out FlatBody body)
        {
            body = null;
            if (index < 0 || index >= this.bodyList.Count)
            {
                return false;
            }

            body = this.bodyList[index];
            return true;
        }

        public void Step(float time, int totalIteration)
        {
            totalIteration = FlatMath.Clamp(totalIteration, FlatWorld.MinIteration, FlatWorld.MaxIteration);

            for (int currentIteration = 0; currentIteration < totalIteration; currentIteration++)
            {
                // 先clear 碰撞的信息
                this.contactPairs.Clear();

                this.StepBodies(time, totalIteration);
                // 宽检测阶段
                this.BroadPhase();
                // 细节检测阶段
                this.NarrowPhase();

            }
        }

        private void BroadPhase()
        {
            /* Collision Step.*/
            for (int i = 0; i < this.bodyList.Count - 1; i++)
            {
                FlatBody bodyA = this.bodyList[i];
                FlatAABB bodyA_aabb = bodyA.GetAABB();
                for (int j = i + 1; j < this.bodyList.Count; j++)
                {
                    FlatBody bodyB = this.bodyList[j];
                    FlatAABB bodyB_aabb = bodyB.GetAABB();
                    if (bodyA.IsStatic && bodyB.IsStatic)
                    {
                        continue;
                    }
                    // 如果AABB都不相交 那么可能不会发生碰撞
                    if (!Collisions.InsertAABB(bodyA_aabb, bodyB_aabb))
                    {
                        continue;
                    }

                    this.contactPairs.Add((i, j));
                }
            }
        }

        private void NarrowPhase()
        {
            // Solve Collision
            for (int i = 0; i < this.contactPairs.Count; i++)
            {
                (int, int) pair = this.contactPairs[i];
                FlatBody bodyA = this.bodyList[pair.Item1];
                FlatBody bodyB = this.bodyList[pair.Item2];

                if (Collisions.Collide(bodyA, bodyB, out FlatVector normal, out float depth))
                {
                    this.SperateBodies(bodyA, bodyB, normal * depth);
                    Collisions.FindContactPoints(bodyA, bodyB,
                        out FlatVector contact1, out FlatVector contact2, out int contactCount);
                    FlatManifold contact = new FlatManifold(bodyA, bodyB, normal, depth, contact1, contact2, contactCount);
                    this.ResolveCollisionWithRotationAndFriction(in contact);
                }
            }
        }

        private void StepBodies(float time, int totalIteration)
        {
            // Movement Step.
            for (int i = 0; i < this.bodyList.Count; i++)
            {
                this.bodyList[i].Step(time, this.gravity, totalIteration);
            }
        }

        private void SperateBodies(FlatBody bodyA, FlatBody bodyB, FlatVector mtv)
        {
            if (bodyA.IsStatic)
            {
                bodyB.Move(mtv);
            }
            else if (bodyB.IsStatic)
            {
                bodyA.Move(-mtv);
            }
            else
            {
                bodyA.Move(-mtv * 0.5f);
                bodyB.Move(mtv * 0.5f);
            }
        }

        public void ResolveCollisionBasic(in FlatManifold contact)
        {
            FlatBody bodyA = contact.bodyA;
            FlatBody bodyB = contact.bodyB;
            FlatVector normal = contact.Normal;
            float depth = contact.Depth;

            FlatVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (FlatMath.Dot(relativeVelocity, normal) > 0f)
            {
                return;
            }

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            // 力的方向是法线的方向 然后大小是相对速度的到法线方向的映射 (有点像做功的一些思想)
            float j = -(1f + e) * FlatMath.Dot(relativeVelocity, normal);
            // 因为normal dot normal 为1 所以不用写 (公式推导)
            j /= bodyA.InvMass + bodyB.InvMass;

            FlatVector impulse = j * normal;
            bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            bodyB.LinearVelocity += impulse * bodyB.InvMass;
        }

        public void ResolveCollisionWithRotation(in FlatManifold contact)
        {
            FlatBody bodyA = contact.bodyA;
            FlatBody bodyB = contact.bodyB;
            FlatVector normal = contact.Normal;
            FlatVector contact1 = contact.Contact1;
            FlatVector contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);
            this.contactList[0] = contact1;
            this.contactList[1] = contact2;

            for (int i = 0; i < contactCount; i++)
            {
                this.impulseList[i] = FlatVector.Zero;
                this.raList[i] = FlatVector.Zero;
                this.rbList[i] = FlatVector.Zero;
            }

            for (int i = 0; i < contactCount; i++)
            {
                FlatVector ra = contactList[i] - bodyA.Position;
                FlatVector rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                FlatVector raPerp = new FlatVector(-ra.Y, ra.X);
                FlatVector rbPerp = new FlatVector(-rb.Y, rb.X);

                FlatVector angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                FlatVector angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                FlatVector relativeVelocity =
                    (bodyB.LinearVelocity + angularLinearVelocityB) -
                    (bodyA.LinearVelocity + angularLinearVelocityA);

                float contactVelocityMag = FlatMath.Dot(relativeVelocity, normal);

                if (contactVelocityMag > 0f)
                {
                    continue;
                }

                float raPerpDotN = FlatMath.Dot(raPerp, normal);
                float rbPerpDotN = FlatMath.Dot(rbPerp, normal);

                float denom = bodyA.InvMass + bodyB.InvMass +
                    (raPerpDotN * raPerpDotN) * bodyA.InvInertia +
                    (rbPerpDotN * rbPerpDotN) * bodyB.InvInertia;

                float j = -(1f + e) * contactVelocityMag;
                j /= denom;
                j /= (float)contactCount;

                impulseList[i] = j * normal;
            }

            for (int i = 0; i < contactCount; i++)
            {
                FlatVector impulse = this.impulseList[i];
                FlatVector ra = this.raList[i];
                FlatVector rb = this.rbList[i];

                bodyA.LinearVelocity += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -FlatMath.Cross(ra, impulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += FlatMath.Cross(rb, impulse) * bodyB.InvInertia;
            }
        }


        public void ResolveCollisionWithRotationAndFriction(in FlatManifold contact)
        {
            FlatBody bodyA = contact.bodyA;
            FlatBody bodyB = contact.bodyB;
            FlatVector normal = contact.Normal;
            FlatVector contact1 = contact.Contact1;
            FlatVector contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float sf = (bodyA.StaticFriction + bodyB.StaticFriction) * 0.5f;
            float df = (bodyA.DynamicFriction + bodyB.DynamicFriction) * 0.5f;

            this.contactList[0] = contact1;
            this.contactList[1] = contact2;

            for (int i = 0; i < contactCount; i++)
            {
                this.impulseList[i] = FlatVector.Zero;
                this.raList[i] = FlatVector.Zero;
                this.rbList[i] = FlatVector.Zero;
                this.frictionImpulseList[i] = FlatVector.Zero;
                this.jList[i] = 0f;
            }

            for (int i = 0; i < contactCount; i++)
            {
                FlatVector ra = contactList[i] - bodyA.Position;
                FlatVector rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                FlatVector raPerp = new FlatVector(-ra.Y, ra.X);
                FlatVector rbPerp = new FlatVector(-rb.Y, rb.X);

                FlatVector angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                FlatVector angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                FlatVector relativeVelocity =
                    (bodyB.LinearVelocity + angularLinearVelocityB) -
                    (bodyA.LinearVelocity + angularLinearVelocityA);

                float contactVelocityMag = FlatMath.Dot(relativeVelocity, normal);

                if (contactVelocityMag > 0f)
                {
                    continue;
                }

                float raPerpDotN = FlatMath.Dot(raPerp, normal);
                float rbPerpDotN = FlatMath.Dot(rbPerp, normal);

                float denom = bodyA.InvMass + bodyB.InvMass +
                    (raPerpDotN * raPerpDotN) * bodyA.InvInertia +
                    (rbPerpDotN * rbPerpDotN) * bodyB.InvInertia;

                float j = -(1f + e) * contactVelocityMag;
                j /= denom;
                j /= (float)contactCount;

                jList[i] = j;

                FlatVector impulse = j * normal;
                impulseList[i] = impulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                FlatVector impulse = impulseList[i];
                FlatVector ra = raList[i];
                FlatVector rb = rbList[i];

                bodyA.LinearVelocity += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -FlatMath.Cross(ra, impulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += FlatMath.Cross(rb, impulse) * bodyB.InvInertia;
            }

            for (int i = 0; i < contactCount; i++)
            {
                FlatVector ra = contactList[i] - bodyA.Position;
                FlatVector rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                FlatVector raPerp = new FlatVector(-ra.Y, ra.X);
                FlatVector rbPerp = new FlatVector(-rb.Y, rb.X);

                FlatVector angularLinearVelocityA = raPerp * bodyA.AngularVelocity;
                FlatVector angularLinearVelocityB = rbPerp * bodyB.AngularVelocity;

                FlatVector relativeVelocity =
                    (bodyB.LinearVelocity + angularLinearVelocityB) -
                    (bodyA.LinearVelocity + angularLinearVelocityA);

                FlatVector tangent = relativeVelocity - FlatMath.Dot(relativeVelocity, normal) * normal;

                if (FlatMath.NearlyEqual(tangent, FlatVector.Zero))
                {
                    continue;
                }
                else
                {
                    tangent = FlatMath.Normalize(tangent);
                }

                float raPerpDotT = FlatMath.Dot(raPerp, tangent);
                float rbPerpDotT = FlatMath.Dot(rbPerp, tangent);

                float denom = bodyA.InvMass + bodyB.InvMass +
                    (raPerpDotT * raPerpDotT) * bodyA.InvInertia +
                    (rbPerpDotT * rbPerpDotT) * bodyB.InvInertia;

                float jt = -FlatMath.Dot(relativeVelocity, tangent);
                jt /= denom;
                jt /= (float)contactCount;

                FlatVector frictionImpulse;
                float j = jList[i];

                if (MathF.Abs(jt) <= j * sf)
                {
                    frictionImpulse = jt * tangent;
                }
                else
                {
                    frictionImpulse = -j * tangent * df;
                }

                this.frictionImpulseList[i] = frictionImpulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                FlatVector frictionImpulse = this.frictionImpulseList[i];
                FlatVector ra = raList[i];
                FlatVector rb = rbList[i];

                bodyA.LinearVelocity += -frictionImpulse * bodyA.InvMass;
                bodyA.AngularVelocity += -FlatMath.Cross(ra, frictionImpulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += frictionImpulse * bodyB.InvMass;
                bodyB.AngularVelocity += FlatMath.Cross(rb, frictionImpulse) * bodyB.InvInertia;
            }
        }
    }
}
