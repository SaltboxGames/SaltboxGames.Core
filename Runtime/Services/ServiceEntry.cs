// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;

namespace SaltboxGames.Core.Services
{
    internal class ServiceEntry
    {
        public ServiceEntry(IService instance)
        {
            Instance = instance;
        }

        public IService Instance { get; }
        public ServiceScope Scope { get; set; }
        public string ScopeName { get; set; }
        public ServiceScopeState State { get; set; } = ServiceScopeState.Registered;
        public Task InitializeTask { get; set; }
    }
}