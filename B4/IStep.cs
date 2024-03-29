﻿// Copyright (c) 2022 dotBunny Inc.
// dotBunny licenses this file to you under the BSL-1.0 license.
// See the LICENSE file in the project root for more information.

namespace B4
{
    public interface IStep
    {
        public string GetID();
        public string GetHeader();
        public void Process();
    }
}