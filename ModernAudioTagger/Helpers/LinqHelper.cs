﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernAudioTagger.Helpers
{
    public static class LinqHelper
    {
        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }
    }
}
