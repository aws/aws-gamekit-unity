// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

namespace AWS.GameKit.Runtime.Features.GameKitGameSaving
{
    /// <summary>
    /// Describes the default Game Saving model to be used by the GameKit SDK
    /// </summary>
    public class DefaultGameSavingModel
    {
        /// <summary>
        /// Create a GameSavingModel struct.
        /// </summary>
        /// <returns>The created GameSavingModel struct.</returns>
        public static GameSavingModel Make()
        {
            GameSavingModel model = new GameSavingModel();
            model.SlotName = string.Empty;
            model.Metadata = string.Empty;
            model.EpochTime = 0;
            model.OverrideSync = false;
            model.Data = IntPtr.Zero;
            model.DataSize = 0;
            model.UrlTimeToLive = GameSavingConstants.S3_PRE_SIGNED_URL_DEFAULT_TIME_TO_LIVE_SECONDS;
            model.ConsistentRead = true;

            return model;
        }
    }
}