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