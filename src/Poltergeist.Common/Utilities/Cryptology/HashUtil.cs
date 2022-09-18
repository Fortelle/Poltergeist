using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Poltergeist.Common.Utilities.Cryptology;

public static class HashUtil
{
    public static string SHAHash(byte[] data)
    {
        using var sha1 = new SHA1CryptoServiceProvider();
        var hashData = sha1.ComputeHash(data);
        var hash = Convert.ToBase64String(hashData);
        return hash;
    }

    //public static int BestCompare(bool[] bits, params bool[][] others)
    //{
    //    var counts = new int[others.Length];
    //    for(var j = 0; j < others.Length; j++)
    //    {
    //        var count = 0;
    //        for (var i = 0; i < bits.Length; i++)
    //        {
    //            if (bits[i] == others[j][i]) count++;
    //        }
    //        counts[j] = count;
    //    }
    //    var max = counts.Max();
    //    var index = Array.IndexOf(counts, max);
    //    return index;
    //}

    public static string BoolsToString(bool[] data)
    {
        var bits = new BitArray(data);
        var bytes = new byte[(bits.Length - 1) / 8 + 1];
        bits.CopyTo(bytes, 0);
        var hash = string.Concat(bytes.Select(x => x.ToString("X2")));
        return hash;
    }

    public static bool[] StringToBools(string hash)
    {
        var bytes = Regex.Matches(hash, "..")
            .Cast<Match>()
            .Select(x => Convert.ToByte(x.Value, 16))
            .ToArray();
        var bits = new BitArray(bytes);
        var bools = new bool[bits.Length];
        bits.CopyTo(bools, 0);
        return bools;
    }

    public static string BitArrayToString(BitArray bits)
    {
        var bytes = new byte[(bits.Length - 1) / 8 + 1];
        bits.CopyTo(bytes, 0);
        var hash = string.Concat(bytes.Select(x => x.ToString("X2")));
        return hash;
    }

    public static BitArray StringToBitArray(string hash)
    {
        var bytes = Regex.Matches(hash, "..")
            .Cast<Match>()
            .Select(x => Convert.ToByte(x.Value, 16))
            .ToArray();
        var bits = new BitArray(bytes);
        return bits;
    }


    public static int GetDistance(BitArray bit1, BitArray bit2)
    {
        var dist = 0;
        for (var i = 0; i < bit1.Length; i++)
        {
            if (bit1[i] != bit2[i]) dist++;
        }
        return dist;
    }

    public static int GetDistance(bool[] arr1, bool[] arr2)
    {
        var diff = 0;
        for (var i = 0; i < arr1.Length; i++)
        {
            if (arr1[i] != arr2[i]) diff++;
        }
        return diff;
    }

    public static int GetDistance(string hash1, string hash2)
    {
        var diff = 0;
        for (var i = 0; i < hash1.Length; i++)
        {
            if (hash1[i] != hash2[i]) diff++;
        }
        return diff;
    }


}
