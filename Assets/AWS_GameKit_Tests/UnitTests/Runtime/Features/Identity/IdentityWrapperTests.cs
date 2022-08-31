// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Runtime.Features.GameKitIdentity;
using AWS.GameKit.Runtime.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class IdentityWrapperTests : GameKitTestBase
    {
        [Test]
        public void IdentityGetUserResponseCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            GetUserResponse getUserResponse = new GetUserResponse()
            {
                CreatedAt = "created at",
                Email = "email",
                FacebookExternalId = "facebooxk external id",
                FacebookRefId = "facebook ref id",
                UpdatedAt = "update at",
                UserId = "user id",
                UserName = "user name"
            };

            IntPtr pGetUserResponse = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(GetUserResponse)));
            Marshal.StructureToPtr<GetUserResponse>(getUserResponse, pGetUserResponse, false);

            GetUserResult result = new GetUserResult();

            // act
            Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => IdentityWrapperTarget.IdentityGetUserResponseCallback(dispatchReceiver, pGetUserResponse));

            // assert
            Assert.AreEqual(getUserResponse.CreatedAt, result.Response.CreatedAt);
            Assert.AreEqual(getUserResponse.Email, result.Response.Email);
            Assert.AreEqual(getUserResponse.FacebookExternalId, result.Response.FacebookExternalId);
            Assert.AreEqual(getUserResponse.FacebookRefId, result.Response.FacebookRefId);
            Assert.AreEqual(getUserResponse.UpdatedAt, result.Response.UpdatedAt);
            Assert.AreEqual(getUserResponse.UserId, result.Response.UserId);
            Assert.AreEqual(getUserResponse.UserName, result.Response.UserName);

            // cleanup
            Marshal.FreeHGlobal(pGetUserResponse);
        }
    }

    public class IdentityWrapperTarget : IdentityWrapper
    {
        public new static void IdentityGetUserResponseCallback(IntPtr dispatchReceiver, IntPtr getUserResponse) => IdentityWrapper.IdentityGetUserResponseCallback(dispatchReceiver, getUserResponse);
    }
}
