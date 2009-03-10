﻿using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    // FIXME: Implement this class
    class TaskInventoryManager : IExtension<Simian>, ITaskInventoryProvider
    {
        Simian server;

        public TaskInventoryManager()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;
        }

        public void Stop()
        {
        }

        public UUID CreateItem(UUID agentID, UUID containerObjectID, string name, string description, InventoryType invType,
            AssetType type, UUID assetID, UUID parentID, PermissionMask ownerMask, PermissionMask nextOwnerMask,
            UUID ownerID, UUID creatorID, UUID transactionID, uint callbackID, bool sendPacket)
        {
            return UUID.Zero;
        }

        public bool RemoveItem(UUID agentID, UUID containerObjectID, UUID itemID)
        {
            return false;
        }

        public bool TryGetAsset(UUID containerObjectID, UUID assetID, out Asset asset)
        {
            asset = null;
            return false;
        }

        public void ForEachItem(UUID containerObjectID, Action<InventoryTaskItem> action)
        {
        }

        public InventoryTaskItem FindItem(UUID containerObjectID, Predicate<InventoryTaskItem> match)
        {
            return null;
        }

        public List<InventoryTaskItem> FindAllItems(UUID containerObjectID, Predicate<InventoryTaskItem> match)
        {
            return new List<InventoryTaskItem>();
        }
    }
}