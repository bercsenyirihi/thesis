using System;

namespace PipeMessages
{
    [Serializable]
    public class PipeMessage
    {
        public PipeMessage()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public ActionType Action { get; set; }
        public string BaseToken { get; set; }
        public string QuoteToken { get; set; }
        public string List { get; set; }
    }

    public enum ActionType
    {
        GetBaseTokenList,
        GetQuoteTokenList,
        GetCurrentData,
        ReturnBaseTokenList,
        ReturnQuoteTokenList,
        ReturnCurrentData
    }
}