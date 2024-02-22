using Chat.Data.Features.Chat;

namespace Chat.Web.Client;

public class VectorClockComparer : IComparer<ChatMessageResponse>
{
    public int Compare(ChatMessageResponse x, ChatMessageResponse y)
    {
        if (x.VectorClock.Count == 0 && y.VectorClock.Count == 0)
        {
            return 0;
        }
        else if (x.VectorClock.Count == 0)
        {
            return -1;
        }
        else if (y.VectorClock.Count == 0)
        {
            return 1;
        }
        else
        {
            var xKeys = x.VectorClock.Keys;
            var yKeys = y.VectorClock.Keys;
            var xIsLess = false;
            var yIsLess = false;
            foreach (var key in xKeys)
            {
                if (!y.VectorClock.ContainsKey(key))
                {
                    xIsLess = true;
                    break;
                }
                else if (x.VectorClock[key] > y.VectorClock[key])
                {
                    xIsLess = true;
                    break;
                }
            }
            foreach (var key in yKeys)
            {
                if (!x.VectorClock.ContainsKey(key))
                {
                    yIsLess = true;
                    break;
                }
                else if (y.VectorClock[key] > x.VectorClock[key])
                {
                    yIsLess = true;
                    break;
                }
            }
            if (xIsLess && yIsLess)
            {
                return 0;
            }
            else if (xIsLess)
            {
                return -1;
            }
            else if (yIsLess)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
