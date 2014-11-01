﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GW2PAO.Data;
using GW2PAO.Data.UserData;
using GW2PAO.ViewModels;
using GW2PAO.ViewModels.Commerce;
using GW2PAO.ViewModels.Commerce.PriceNotification;

namespace GW2PAO.Controllers.Interfaces
{
    public interface ICommerceController
    {
        /// <summary>
        /// The interval by which to refresh the dungeon reset state (in ms)
        /// </summary>
        int RefreshInterval { get; set; }

        /// <summary>
        /// The commers user data
        /// </summary>
        CommerceUserData UserData { get; }

        /// <summary>
        /// Collection of item prices for the price watch tracker and notifications
        /// </summary>
        ObservableCollection<ItemPriceViewModel> ItemPrices { get; }

        /// <summary>
        /// Collection of price notifications currently shown to the user
        /// </summary>
        ObservableCollection<PriceNotificationViewModel> PriceNotifications { get; }

        /// <summary>
        /// Starts the automatic refresh
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the automatic refresh
        /// </summary>
        void Stop();
    }
}
