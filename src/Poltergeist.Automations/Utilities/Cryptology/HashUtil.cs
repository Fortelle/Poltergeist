using System.Collections;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Poltergeist.Automations.Utilities.Cryptology;

public static class HashUtil
{
    public static string SHAHash(byte[] data)
    {
        var hashData = SHA1.HashData(data);
        var hash = Convert.ToBase64String(hashData);
        return hash;
    }

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
        var length = Math.Min(bit1.Length, bit2.Length);
        for (var i = 0; i < length; i++)
        {
            if (bit1[i] != bit2[i])
            {
                dist++;
            }
        }
        return dist;
    }

    public static int GetDistance(bool[] arr1, bool[] arr2)
    {
        var diff = 0;
        var length = Math.Min(arr1.Length, arr2.Length);
        for (var i = 0; i < length; i++)
        {
            if (arr1[i] != arr2[i])
            {
                diff++;
            }
        }
        return diff;
    }

    public static int GetDistance(string hash1, string hash2)
    {
        var diff = 0;
        var length = Math.Min(hash1.Length, hash2.Length);
        for (var i = 0; i < length; i++)
        {
            if (hash1[i] != hash2[i])
            {
                diff++;
            }
        }
        return diff;
    }


}
