﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GW2PAO.API.Data;
using GW2PAO.Models;
using GW2PAO.PresentationCore;
using NLog;

namespace GW2PAO.ViewModels.DungeonTracker
{
    public class PathViewModel : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Default logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DungeonSettings userSettings;

        /// <summary>
        /// The primary model object containing the path's information
        /// </summary>
        public DungeonPath PathModel { get; private set; }

        /// <summary>
        /// The paths's ID
        /// </summary>
        public Guid PathId { get { return this.PathModel.ID; } }

        /// <summary>
        /// The paths's display text name
        /// </summary>
        public string DisplayName { get { return this.PathModel.PathDisplayText; } }

        /// <summary>
        /// The paths's nickname
        /// </summary>
        public string NickName { get { return this.PathModel.Nickname; } }

        /// <summary>
        /// The paths's reward Gold component
        /// </summary>
        public int RewardGold { get { return (int)this.PathModel.GoldReward; } }

        /// <summary>
        /// The paths's reward Silver component
        /// </summary>
        public int RewardSilver { get { return (int)((this.PathModel.GoldReward - RewardGold) * 100); } }

        /// <summary>
        /// True if the path has been completed, else false
        /// </summary>
        public bool IsCompleted
        {
            get { return this.userSettings.CompletedPaths.Contains(this.PathId); }
            set
            {
                if (value && !this.userSettings.CompletedPaths.Contains(this.PathId))
                {
                    logger.Debug("Adding \"{0}\" to CompletedPaths", this.PathId);
                    this.userSettings.CompletedPaths.Add(this.PathModel.ID);
                    this.RaisePropertyChanged();
                }
                else
                {
                    logger.Debug("Removing \"{0}\" from CompletedPaths", this.PathId);
                    if (this.userSettings.CompletedPaths.Remove(this.PathId))
                        this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="userSettings"></param>
        public PathViewModel(DungeonPath path, DungeonSettings userSettings)
        {
            this.PathModel = path;
            this.userSettings = userSettings;
        }
    }
}