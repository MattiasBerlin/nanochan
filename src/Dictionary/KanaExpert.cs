using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoChan.Dictionary
{
    public enum KanaVowelType { AKana, EKana, OKana, IKana, UKana, UnknownKana }

    class Kana
    {
        public List<char> KanaList;
        private char IKana, UKana;

        public Kana(List<char> kanaList, char iKana, char uKana)
        {
            KanaList = kanaList;
            IKana = iKana;
            UKana = uKana;
        }

        public Kana(List<char> kanaList)
        {
            KanaList = kanaList;
        }

        public bool Contains(char c)
        {
            return KanaList.Contains(c);
        }

        public char GetIVariant()
        {
            return IKana;
        }

        public char GetUVariant()
        {
            return UKana;
        }
    }

    class VowelKana
    {
        public List<char> KanaList { get; set; }
        public KanaVowelType VowelType { get; set; }

        public VowelKana(List<char> kanaList, KanaVowelType vowelType)
        {
            KanaList = kanaList;
            VowelType = vowelType;
        }
    }

    static class KanaExpert
    {
        // TODO Add hira -> kata and vice versa

        private static readonly Kana AKana = new Kana(new List<char> { 'あ', 'え', 'お', 'い', 'う' }, 'い', 'う');
        private static readonly Kana HKana = new Kana(new List<char> { 'は', 'へ', 'ほ', 'ひ', 'ふ' }, 'ひ', 'ふ');
        private static readonly Kana BKana = new Kana(new List<char> { 'ば', 'べ', 'ぼ', 'び', 'ぶ' }, 'び', 'ぶ');
        private static readonly Kana PKana = new Kana(new List<char> { 'ぱ', 'ぺ', 'ぽ', 'ぴ', 'ぷ' }, 'ぴ', 'ぷ');
        private static readonly Kana RKana = new Kana(new List<char> { 'ら', 'れ', 'ろ', 'り', 'る' }, 'り', 'る');
        private static readonly Kana MKana = new Kana(new List<char> { 'ま', 'め', 'も', 'み', 'む' }, 'み', 'む');
        private static readonly Kana NKana = new Kana(new List<char> { 'な', 'ね', 'の', 'に', 'ぬ' }, 'に', 'ぬ');
        private static readonly Kana TKana = new Kana(new List<char> { 'た', 'て', 'と', 'ち', 'つ' }, 'ち', 'つ');
        private static readonly Kana DKana = new Kana(new List<char> { 'だ', 'で', 'ど', 'ぢ', 'づ' }, 'ぢ', 'づ');
        private static readonly Kana SKana = new Kana(new List<char> { 'さ', 'せ', 'そ', 'し', 'す' }, 'し', 'す');
        private static readonly Kana ZKana = new Kana(new List<char> { 'ざ', 'ぜ', 'ぞ', 'じ', 'ず' }, 'じ', 'ず');
        private static readonly Kana YKana = new Kana(new List<char> { 'や', 'よ', 'ゆ' });
        private static readonly Kana WKana = new Kana(new List<char> { 'わ', 'を' });

        private static readonly List<Kana> ConsonantKana = new List<Kana> { AKana, HKana, BKana, PKana, RKana, MKana, NKana, TKana, DKana, SKana, ZKana, YKana, WKana };

        private static readonly VowelKana VowelAKana = new VowelKana(new List<char> { 'あ', 'は', 'ば', 'ぱ', 'ら', 'ま', 'な', 'た', 'だ', 'さ', 'ざ', 'か', 'が', 'や', 'わ' }, KanaVowelType.AKana);
        private static readonly VowelKana VowelEKana = new VowelKana(new List<char> { 'え', 'へ', 'べ', 'ぺ', 'れ', 'め', 'ね', 'て', 'で', 'せ', 'ぜ', 'け', 'げ' }, KanaVowelType.EKana);
        private static readonly VowelKana VowelOKana = new VowelKana(new List<char> { 'お', 'ほ', 'ぼ', 'ぽ', 'ろ', 'も', 'の', 'と', 'ど', 'そ', 'ぞ', 'こ', 'ご', 'よ', 'を' }, KanaVowelType.OKana);
        private static readonly VowelKana VowelIKana = new VowelKana(new List<char> { 'い', 'ひ', 'び', 'ぴ', 'り', 'み', 'に', 'ち', 'ぢ', 'し', 'じ', 'き', 'ぎ' }, KanaVowelType.IKana);
        private static readonly VowelKana VowelUKana = new VowelKana(new List<char> { 'う', 'ふ', 'ぶ', 'ぷ', 'る', 'む', 'ぬ', 'つ', 'づ', 'す', 'ず', 'く', 'ぐ', 'ゆ' }, KanaVowelType.UKana);

        private static readonly List<VowelKana> VowelKana = new List<VowelKana> { VowelAKana, VowelEKana, VowelOKana, VowelIKana, VowelUKana };

        /*private static readonly List<char> VowelAKana = new List<char> { 'あ', 'は', 'ば', 'ぱ', 'ら', 'ま', 'な', 'た', 'だ', 'さ', 'ざ', 'か', 'が', 'や', 'わ' };
        private static readonly List<char> VowelEKana = new List<char> { 'え', 'へ', 'べ', 'ぺ', 'れ', 'め', 'ね', 'て', 'で', 'せ', 'ぜ', 'け', 'げ' };
        private static readonly List<char> VowelOKana = new List<char> { 'お', 'ほ', 'ぼ', 'ぽ', 'ろ', 'も', 'の', 'と', 'ど', 'そ', 'ぞ', 'こ', 'ご', 'よ', 'を' };
        private static readonly List<char> VowelIKana = new List<char> { 'い', 'ひ', 'び', 'ぴ', 'り', 'み', 'に', 'ち', 'ぢ', 'し', 'じ', 'き', 'ぎ' };
        private static readonly List<char> VowelUKana = new List<char> { 'う', 'ふ', 'ぶ', 'ぷ', 'る', 'む', 'ぬ', 'つ', 'づ', 'す', 'ず', 'く', 'ぐ', 'ゆ' };

        public enum KanaType { AKana, EKana, OKana, IKana, UKana, UnknownKana }

        // Is this method needed?
        public static KanaType GetKanaVowelType(char c)
        {
            KanaType kanaType = KanaType.UnknownKana;

            if (VowelAKana.Contains(c))
            {
                kanaType = KanaType.AKana;
            }
            else if (VowelEKana.Contains(c))
            {
                kanaType = KanaType.EKana;
            }
            else if (VowelOKana.Contains(c))
            {
                kanaType = KanaType.OKana;
            }
            else if (VowelIKana.Contains(c))
            {
                kanaType = KanaType.IKana;
            }
            else if (VowelUKana.Contains(c))
            {
                kanaType = KanaType.UKana;
            }

            return kanaType;
        }*/

        public static char? GetCorrespondingUHiragana(char c)
        {
            foreach (Kana kana in ConsonantKana)
            {
                if (kana.Contains(c))
                {
                    return kana.GetUVariant();
                }
            }

            return null;
        }

        public static bool IsKana(char c)
        {
            return IsHiragana(c) || IsKatakana(c);
        }

        public static bool IsHiragana(char c)
        {
            return c >= '\u3041' && c <= '\u3096';
        }

        public static bool IsKatakana(char c)
        {
            return c >= '\u30A1' && c <= '\u30F7';
        }

        public static bool ContainsOnlyKana(string s)
        {
            foreach (char c in s)
            {
                if (!IsKana(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
