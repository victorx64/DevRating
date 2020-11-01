// Copyright (c) 2019-present Viktor Semenov
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace DevRating.VersionControl
{
    public sealed class CachedPatches : Patches
    {
        private readonly Patches _origin;
        private IEnumerable<FilePatch>? _items;

        public CachedPatches(Patches origin)
        {
            _origin = origin;
        }

        public IEnumerable<FilePatch> Items()
        {
            lock (_origin)
            {
                return _items ??= _origin.Items();
            }
        }
    }
}