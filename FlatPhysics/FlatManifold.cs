using System;


namespace FlatPhysics
{
    public readonly struct FlatManifold
    {
        public readonly FlatBody bodyA;
        public readonly FlatBody bodyB;
        public readonly FlatVector Normal;
        public readonly float Depth;
        public readonly FlatVector Contact1;
        public readonly FlatVector Contact2;
        public readonly int ContactCount;

        public FlatManifold(
            FlatBody bodyA,FlatBody bodyB,
            FlatVector normal, float depth,
            FlatVector contact1, FlatVector contact2, int contactCount)
        {
            this.bodyA = bodyA;
            this.bodyB = bodyB;
            this.Normal = normal;
            this.Depth = depth;
            this.ContactCount = contactCount;
            this.Contact1 = contact1;
            this.Contact2 = contact2;
        }
    }
}
