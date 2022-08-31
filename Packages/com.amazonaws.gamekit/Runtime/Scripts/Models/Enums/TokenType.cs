// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Runtime.Models
{
    public enum TokenType : uint
    {
        AccessToken,
        RefreshToken,
        IdToken,
        IamSessionToken,

        // TokenType_COUNT should be the last item in the enum, it is used for providing the total number of enums
        TokenType_COUNT
    }
}
