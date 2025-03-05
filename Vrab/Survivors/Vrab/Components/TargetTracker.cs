using System;
using R2API.Networking.Interfaces;

namespace Vrab {
    public class TargetTracker : HurtboxTracker {
        public override void Start()
        {
            base.targetingIndicatorPrefab = Survivor.TargetPainter;
            base.maxSearchAngle = 30f;
            base.maxSearchDistance = 70f;
            base.targetType = TargetType.Enemy;
            base.userIndex = TeamIndex.Player;
            base.Start();
        }
    }

    public class SyncVrabTarget : INetMessage
    {
        Transform target;
        GameObject tracker;
        public SyncVrabTarget(GameObject obj, Transform transform) {
            target = transform;
            tracker = obj;
        }
        public SyncVrabTarget() {

        }
        void ISerializableObject.Deserialize(NetworkReader reader)
        {
            target = reader.ReadHurtBoxReference().ResolveHurtBox().transform;
            tracker = reader.ReadGameObject();
        }

        void ISerializableObject.Serialize(NetworkWriter writer)
        {
            HurtBoxReference hb = HurtBoxReference.FromHurtBox(target.GetComponent<HurtBox>());
            writer.Write(hb);
            writer.Write(tracker);
        }

        void INetMessage.OnReceived()
        {
            tracker.GetComponent<TargetTracker>().target = target;
        }
    }
}