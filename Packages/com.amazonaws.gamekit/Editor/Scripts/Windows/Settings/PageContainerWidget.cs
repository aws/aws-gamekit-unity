// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// A GUI element which displays a page and handles transition logic when changing to a different page.
    /// </summary>
    public class PageContainerWidget
    {
        public Page CurrentPage { get; private set; }

        public void OnGUI()
        {
            if (CurrentPage != null)
            {
                CurrentPage.OnGUI();
            }
        }

        public void ChangeTo(Page nextPage)
        {
            if (CurrentPage == nextPage)
            {
                return;
            }

            if (CurrentPage != null)
            {
                CurrentPage.OnNavigatedFrom();
            }

            CurrentPage = nextPage;
            nextPage.OnNavigatedTo();
        }

        public void CloseWindow()
        {
            if (CurrentPage != null)
            {
                CurrentPage.OnNavigatedFrom();
            }
        }
    }
}
