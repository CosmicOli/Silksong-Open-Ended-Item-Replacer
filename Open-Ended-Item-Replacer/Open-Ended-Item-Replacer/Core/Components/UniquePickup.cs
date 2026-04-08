using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCherry.Localization;
using UnityEngine;

namespace Open_Ended_Item_Replacer.Core.Components
{
    public struct UniquePickup
    {
        private LocalisedString popupName;
        public string PopupName
        {
            get
            {
                return popupName;
            }
        }

        private LocalisedString description;
        public string Description
        {
            get
            {
                return description;
            }
        }

        // Currently unused; borrowed from ShopItem for when savedItem.GetSavedAmount() > 0, but this won't be relevant here
        // Keeping it around in case it is wanted for a similar purpose handled outside of this struct
        private LocalisedString descriptionMultiple;
        public string DescriptionMultiple
        {
            get
            {
                return descriptionMultiple;
            }
        }

        public Sprite PopupIcon
        {
            get;
            private set;
        }

        public UniquePickup(LocalisedString PopupName, LocalisedString Description, LocalisedString DescriptionMultiple, Sprite PopupIcon)
        {
            this.popupName = PopupName;
            this.description = Description;
            this.descriptionMultiple = DescriptionMultiple;
            this.PopupIcon = PopupIcon;
        }
    }
}
