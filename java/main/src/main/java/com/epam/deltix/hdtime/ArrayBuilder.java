/*
 * Copyright 2021 EPAM Systems, Inc
 *
 * See the NOTICE file distributed with this work for additional information
 * regarding copyright ownership. Licensed under the Apache License,
 * Version 2.0 (the "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
package com.epam.deltix.hdtime;

import java.lang.reflect.Array;
import java.util.ArrayList;

class ArrayBuilder<T> {
    final ArrayList<T> items = new ArrayList<>();

    ArrayBuilder<T> add(T f) {
        items.add(f);
        return this;
    }

    protected T[] getFields(Class<? extends T> newType) {
        return items.toArray((T[])Array.newInstance(newType, items.size()));
    }

    void clear() {
        items.clear();
    }
}