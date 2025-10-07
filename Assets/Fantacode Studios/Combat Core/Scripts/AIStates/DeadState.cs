using FS_Core;
using FS_ThirdPerson;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS_CombatCore
{
    public class DeadState : State<CombatAIController>, IAIState
    {
        public AIStates StateKey => AIStates.Dead;

        public override void Enter(CombatAIController owner)
        {
            CombatAIManager.i.RemoveCombatAI(owner);

            owner.NavAgent.enabled = false;

            if (owner.DropWeaponOnDeath)
            {
                var itemEquipper = owner.GetComponent<ItemEquipper>();
                var equippedItem = itemEquipper.EquippedItemObject;
                itemEquipper.DropItem(destroyItem: false);
                if (EquipSettings.i.EquipHotspot != null)
                {
                    var equipHotspot = Instantiate(EquipSettings.i.EquipHotspot, equippedItem.transform);
                    equipHotspot.transform.localRotation = Quaternion.identity;
                    equipHotspot.transform.localPosition = Vector3.zero;
                    equipHotspot.item = equippedItem.itemData;
                    equipHotspot.itemObject = equippedItem;

                    var itemCollider =  equippedItem.GetComponent<Collider>();
                }
            }
        }
    }
}