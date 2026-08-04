using System;

namespace Vrab
{
    public class CopyOwnerInventory : MonoBehaviour
    {
        public Inventory ownerInventory;
        public CharacterMaster self;
        private ItemTag[] blacklistedTags = new ItemTag[] {
                ItemTag.HoldoutZoneRelated, 
                ItemTag.InteractableRelated, 
                ItemTag.OnStageBeginEffect, 
                ItemTag.PowerShape, 
                ItemTag.ObjectiveRelated, 
                ItemTag.ObliterationRelated, 
                ItemTag.CannotCopy,
                ItemTag.Healing
            };

        public void Start()
        {
            self = GetComponent<CharacterMaster>();
        }

        public void FixedUpdate()
        {
            if (!ownerInventory)
            {
                if (self.minionOwnership && self.minionOwnership.ownerMaster)
                {
                    ownerInventory = self.minionOwnership.ownerMaster.inventory;
                    ownerInventory.onInventoryChanged += MirrorInventory;
                    MirrorInventory();
                }
            }
        }

        public void OnDestroy() {
            if (ownerInventory) {
                ownerInventory.onInventoryChanged -= MirrorInventory;
            }
        }

        private void MirrorInventory()
        {
            if (!self || !self.inventory || !ownerInventory || !NetworkServer.active)
            {
                return;
            }
            
            self.inventory.CopyItemsFrom(ownerInventory, ItemFilter);
            self.inventory.GiveItemPermanent(Survivor.SimulMarker);
            self.inventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, 10);
            self.inventory.GiveItemPermanent(RoR2Content.Items.BoostHp, 250);
            self.inventory.GiveItemPermanent(RoR2Content.Items.MinionLeash);
        }

        private bool ItemFilter(ItemIndex index)
        {
            ItemDef item = ItemCatalog.GetItemDef(index);
            if (!item) return false;
            if (item.tier == ItemTier.NoTier) return false;

            foreach (ItemTag tag in blacklistedTags)
            {
                if (item.ContainsTag(tag))
                {
                    return false;
                }
            }

            return true;
        }
    }
}