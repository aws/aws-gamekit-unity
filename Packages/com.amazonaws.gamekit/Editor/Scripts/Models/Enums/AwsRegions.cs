// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Editor.Models
{
    public enum AwsRegion
    {
        [AwsRegionData(regionKey: "us-east-1", regionDescription: "us-east-1: US East (N. Virginia)", isSupported: true)]
        US_EAST_1,

        [AwsRegionData(regionKey: "us-east-2", regionDescription: "us-east-2: US East (Ohio)", isSupported: true)]
        US_EAST_2,

        [AwsRegionData(regionKey: "us-west-1", regionDescription: "us-west-1: US West (N. California)", isSupported: true)]
        US_WEST_1,

        [AwsRegionData(regionKey: "us-west-2", regionDescription: "us-west-2: US West (Oregon)", isSupported: true)]
        US_WEST_2,

        [AwsRegionData(regionKey: "af-south-1", regionDescription: "af-south-1: Africa (Cape Town)", isSupported: false)]
        AF_SOUTH_1,

        [AwsRegionData(regionKey: "ap-east-1", regionDescription: "ap-east-1: Asia Pacific (Hong Kong)", isSupported: false)]
        AP_EAST_1,

        [AwsRegionData(regionKey: "ap-south-1", regionDescription: "ap-south-1: Asia Pacific (Mumbai)", isSupported: true)]
        AP_SOUTH_1,

        [AwsRegionData(regionKey: "ap-northeast-3", regionDescription: "ap-northeast-3: Asia Pacific (Osaka)", isSupported: false)]
        AP_NORTHEAST_3,

        [AwsRegionData(regionKey: "ap-northeast-2", regionDescription: "ap-northeast-2: Asia Pacific (Seoul)", isSupported: true)]
        AP_NORTHEAST_2,

        [AwsRegionData(regionKey: "ap-southeast-1", regionDescription: "ap-southeast-1: Asia Pacific (Singapore)", isSupported: true)]
        AP_SOUTHEAST_1,

        [AwsRegionData(regionKey: "ap-southeast-2", regionDescription: "ap-southeast-2: Asia Pacific (Sydney)", isSupported: true)]
        AP_SOUTHEAST_2,

        [AwsRegionData(regionKey: "ap-northeast-1", regionDescription: "ap-northeast-1: Asia Pacific (Tokyo)", isSupported: true)]
        AP_NORTHEAST_1,

        [AwsRegionData(regionKey: "ca-central-1", regionDescription: "ca-central-1: Canada (Central)", isSupported: true)]
        CA_CENTRAL_1,

        [AwsRegionData(regionKey: "eu-central-1", regionDescription: "eu-central-1: Europe (Frankfurt)", isSupported: true)]
        EU_CENTRAL_1,

        [AwsRegionData(regionKey: "eu-west-1", regionDescription: "eu-west-1: Europe (Ireland)", isSupported: true)]
        EU_WEST_1,

        [AwsRegionData(regionKey: "eu-west-2", regionDescription: "eu-west-2: Europe (London)", isSupported: true)]
        EU_WEST_2,

        [AwsRegionData(regionKey: "eu-south-1", regionDescription: "eu-south-1: Europe (Milan)", isSupported: false)]
        EU_SOUTH_1,

        [AwsRegionData(regionKey: "eu-west-3", regionDescription: "eu-west-3: Europe (Paris)", isSupported: true)]
        EU_WEST_3,

        [AwsRegionData(regionKey: "eu-north-1", regionDescription: "eu-north-1: Europe (Stockholm)", isSupported: true)]
        EU_NORTH_1,

        [AwsRegionData(regionKey: "me-south-1", regionDescription: "me-south-1: Middle East (Bahrain)", isSupported: true)]
        ME_SOUTH_1,

        [AwsRegionData(regionKey: "sa-east-1", regionDescription: "sa-east-1: South America (Sao Paulo)", isSupported: true)]
        SA_EAST_1
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class AwsRegionData : Attribute
    {
        public readonly string RegionKey;
        public readonly string RegionDescription;
        public readonly bool IsSupported;

        public AwsRegionData(string regionKey, string regionDescription, bool isSupported)
        {
            RegionKey = regionKey;
            RegionDescription = regionDescription;
            IsSupported = isSupported;
        }
    }

    /// <summary>
    /// Extension methods for <c>AwsRegion</c> which give access to it's enum metadata.
    /// </summary>
    /// <example>
    /// This shows how to use the extension methods.
    /// <code>
    ///     // On the enum class:
    ///     AwsRegion.AP_SOUTHEAST_2.GetRegionKey();
    ///
    ///     // On an enum variable:
    ///     AwsRegion myRegion = AwsRegion.US_EAST_1
    ///     myRegion.GetRegionKey();
    /// </code>
    /// </example>
    public static class AwsRegionConverter
    {
        public static string GetRegionKey(this AwsRegion awsRegion)
        {
            return awsRegion.GetAttribute<AwsRegionData>().RegionKey;
        }

        public static string GetRegionDescription(this AwsRegion awsRegion)
        {
            return awsRegion.GetAttribute<AwsRegionData>().RegionDescription;
        }

        public static bool IsRegionSupported(this AwsRegion awsRegion)
        {
            return awsRegion.GetAttribute<AwsRegionData>().IsSupported;
        }
    }
}
