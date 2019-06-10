﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public static class SC_ExtensionMethods {

    public static void Shuffle<T> (this IList<T> list) {

        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

        int n = list.Count;

        while (n > 1) {

            byte[] box = new byte[1];

            do
                provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));

            int k = (box[0] % n);

            n--;

            T value = list[k];
            list[k] = list[n];
            list[n] = value;

        }

    }

    public static int I (this int i, bool b) {

        return (b ? 1 : -1) * i;

    }

    public static float F (this float f, bool b) {

        return (b ? 1 : -1) * f;

    }

    public static int ReduceWithMin (this int value, int reduction) {

        return Mathf.Max(0, value - reduction);

    }

}
