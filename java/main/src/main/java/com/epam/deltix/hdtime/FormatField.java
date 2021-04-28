package com.epam.deltix.hdtime;

abstract class FormatField {
    //static int hash(String s) { return 0; }
    // NOTE: TODO: Possible that we define Field as Functional Interface, just to test the performance difference

    @Override
    public boolean equals(Object other) { return this == other || other instanceof FormatField && other.hashCode() == this.hashCode(); }
    @Override
    public int hashCode() { return getClass().getName().hashCode(); }
}
