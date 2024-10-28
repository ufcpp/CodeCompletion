﻿using System.Numerics;

namespace CodeCompletion.Text;

/// <summary>
/// トークン。
/// </summary>
public struct Token()
{
    private const int InitialCapacity = 32;

    public int Written;
    public char[] Text = new char[InitialCapacity];

    /// <summary>
    /// 中身。
    /// </summary>
    public readonly ReadOnlySpan<char> Span => Text.AsSpan(0, Written);

    /// <summary>
    /// <paramref name="position"/> の位置に <paramref name="s"/> を挿入する。
    /// </summary>
    public void Insert(int position, ReadOnlySpan<char> s)
    {
        System.Diagnostics.Debug.Assert(position <= Written);

        EnsureCapacity(Written + s.Length);

        Text.AsSpan(position, Written).CopyTo(Text.AsSpan(position + s.Length));

        s.CopyTo(Text.AsSpan(position));
        Written += s.Length;
    }

    private void EnsureCapacity(int capacity)
    {
        if (capacity >= Text.Length)
        {
            var len = BitOperations.RoundUpToPowerOf2((uint)capacity);

            var newLine = new char[len];
            Text.CopyTo(newLine, 0);
            Text = newLine;
        }
    }

    /// <summary>
    /// 中身を丸ごと <paramref name="s"/> に置き換える。
    /// </summary>
    public void Replace(ReadOnlySpan<char> s)
    {
        EnsureCapacity(s.Length);
        s.CopyTo(Text.AsSpan());
        Text.AsSpan(s.Length).Clear();
        Written = s.Length;
    }
}
