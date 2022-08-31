// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.Collections.Generic;

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public class LazyLoadedTextureForCurrentResolutionTests
    {
        const int TEST_WIDTH = 900;
        const int TEST_WIDTH_2K = 1900;
        const int TEST_WIDTH_4K = 3900;
        const int TEST_WIDTH_8K = 7900;
        const int TEST_HEIGHT = 500;
        const string TEST_PATH = "test_path";
        const string TEST_PATH_2K = "test_path-2k";
        const string TEST_PATH_4K = "test_path-4k";
        const string TEST_PATH_8K = "test_path-8k";

        [Test]
        public void Get_WhenAt8KResolutionAndNoTexturesFound_AttemptsToLoadEachTextureStartingWithTheRequested()
        {
            // arrange
            Resolution res = new Resolution
            {
                width = TEST_WIDTH_8K,
                height = TEST_HEIGHT
            };

            List<string> pathsOUT = new List<string>();
            LazyLoadedResourceForCurrentResolution<Texture> target = new LazyLoadedResourceForCurrentResolution<Texture>(TEST_PATH, res, (pathIN) =>
            {
                pathsOUT.Add(pathIN);
                return null;
            });

            // act
            target.Get();

            // assert
            Assert.AreEqual(4, pathsOUT.Count);
            Assert.AreEqual(TEST_PATH_8K, pathsOUT[0]);
            Assert.AreEqual(TEST_PATH_4K, pathsOUT[1]);
            Assert.AreEqual(TEST_PATH_2K, pathsOUT[2]);
            Assert.AreEqual(TEST_PATH, pathsOUT[3]);
        }

        [Test]
        public void Get_WhenAt4KResolutionAndNoTexturesFound_AttemptsToLoadEachTextureStartingWithTheRequested()
        {
            // arrange
            Resolution res = new Resolution
            {
                width = TEST_WIDTH_4K,
                height = TEST_HEIGHT
            };

            List<string> pathsOUT = new List<string>();
            LazyLoadedResourceForCurrentResolution<Texture> target = new LazyLoadedResourceForCurrentResolution<Texture>(TEST_PATH, res, (pathIN) =>
            {
                pathsOUT.Add(pathIN);
                return null;
            });

            // act
            target.Get();

            // assert
            Assert.AreEqual(3, pathsOUT.Count);
            Assert.AreEqual(TEST_PATH_4K, pathsOUT[0]);
            Assert.AreEqual(TEST_PATH_2K, pathsOUT[1]);
            Assert.AreEqual(TEST_PATH, pathsOUT[2]);
        }

        [Test]
        public void Get_WhenAt2KResolutionAndNoTexturesFound_AttemptsToLoadEachTextureStartingWithTheRequested()
        {
            // arrange
            Resolution res = new Resolution
            {
                width = TEST_WIDTH_2K,
                height = TEST_HEIGHT
            };

            List<string> pathsOUT = new List<string>();
            LazyLoadedResourceForCurrentResolution<Texture> target = new LazyLoadedResourceForCurrentResolution<Texture>(TEST_PATH, res, (pathIN) =>
            {
                pathsOUT.Add(pathIN);
                return null;
            });

            // act
            target.Get();

            // assert
            Assert.AreEqual(2, pathsOUT.Count);
            Assert.AreEqual(TEST_PATH_2K, pathsOUT[0]);
            Assert.AreEqual(TEST_PATH, pathsOUT[1]);
        }

        [Test]
        public void Get_WhenAt1KResolutionAndNoTexturesFound_AttemptsToLoadOnlyThe1KTexture()
        {
            // arrange
            Resolution res = new Resolution
            {
                width = TEST_WIDTH,
                height = TEST_HEIGHT
            };

            List<string> pathsOUT = new List<string>();
            LazyLoadedResourceForCurrentResolution<Texture> target = new LazyLoadedResourceForCurrentResolution<Texture>(TEST_PATH, res, (pathIN) =>
            {
                pathsOUT.Add(pathIN);
                return null;
            });

            // act
            target.Get();

            // assert
            Assert.AreEqual(1, pathsOUT.Count);
            Assert.AreEqual(TEST_PATH, pathsOUT[0]);
        }

        [Test]
        public void Get_WhenAt8KResolution_ReturnsOnceTextureFound()
        {
            // arrange
            Resolution res = new Resolution
            {
                width = TEST_WIDTH_8K,
                height = TEST_HEIGHT
            };

            List<string> pathsOUT = new List<string>();
            LazyLoadedResourceForCurrentResolution<Texture> target = new LazyLoadedResourceForCurrentResolution<Texture>(TEST_PATH, res, (pathIN) =>
            {
                pathsOUT.Add(pathIN);
                return Resources.Load<Texture>("Textures/FeatureStatus-Success");
            });

            // act
            target.Get();

            // assert
            Assert.AreEqual(1, pathsOUT.Count);
            Assert.AreEqual(TEST_PATH_8K, pathsOUT[0]);
        }
    }
}
