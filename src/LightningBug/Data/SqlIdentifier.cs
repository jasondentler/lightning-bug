using System;
using System.Collections.Generic;

namespace LightningBug.Data
{
    public static class SqlIdentifier
    {

        public static string[] Parse(string objectId)
        {
            return new Parser(objectId).Parts.ToArray();
        }

        private class Parser
        {
            public readonly Queue<string> Parts = new Queue<string>();
            public string CurrentPart;
            public readonly string Id;
            public IParserState CurrentState;
            public int Location;
            public char CurrentCharacter {  get { return Id[Location]; } }

            public char? PeekCharacter
            {
                get
                {
                    return Location + 1 < Id.Length
                        ? Id[Location + 1]
                        : (char?) null;
                }
            }

            public Parser(string id)
            {
                Id = id;
                CurrentState = new InitialState();
                while (Location < Id.Length)
                    CurrentState.Process(this);
                if (!string.IsNullOrEmpty(CurrentPart))
                    Parts.Enqueue(CurrentPart);
            }
        }

        private interface IParserState
        {
            void Process(Parser parser);
        }

        private class NormalState : IParserState
        {
            public virtual void Process(Parser parser)
            {
                switch (parser.CurrentCharacter)
                {
                    case '.':
                        parser.Parts.Enqueue(parser.CurrentPart);
                        parser.CurrentPart = "";
                        break;
                    case '[':
                        if (string.IsNullOrEmpty(parser.CurrentPart))
                            parser.CurrentState = new InsideBrackets();
                        else
                            throw new ParserException("Unexpected '[' at character " + parser.Location + " of \"" +
                                                      parser.Id + "\"");
                        break;
                    case '`':
                        if (string.IsNullOrEmpty(parser.CurrentPart))
                            parser.CurrentState = new InsideBackticks();
                        else
                            throw new ParserException("Unexpected '`' at character " + parser.Location + " of \"" +
                                                      parser.Id + "\"");
                        break;
                    case '"':
                        if (string.IsNullOrEmpty(parser.CurrentPart))
                            parser.CurrentState = new InsideDoubleQuotes();
                        else
                            throw new ParserException("Unexpected '\"' at character " + parser.Location + " of \"" +
                                                      parser.Id + "\"");
                        break;
                    default:
                        parser.CurrentPart += parser.CurrentCharacter;
                        break;
                }
                parser.Location++;
            }
        }

        private class InitialState : NormalState
        {
            public override void Process(Parser parser)
            {
                if (parser.CurrentCharacter == '.')
                    throw new ParserException("SQL identifier can't start with '.'");
                base.Process(parser);
            }
        }

        private class InsideBrackets : IParserState
        {
            public void Process(Parser parser)
            {
                switch (parser.CurrentCharacter)
                {
                    case ']':
                        switch (parser.PeekCharacter)
                        {
                            case ']':
                                parser.CurrentPart += parser.CurrentCharacter;
                                parser.Location++; // Advance an extra character...
                                break;
                            default:
                                parser.CurrentState = new NormalState();
                                break;
                        }
                        break;
                    default:
                        parser.CurrentPart += parser.CurrentCharacter;
                        break;
                }
                parser.Location++;
            }
        }

        private class InsideBackticks : IParserState
        {
            public void Process(Parser parser)
            {
                switch (parser.CurrentCharacter)
                {
                    case '`':
                        parser.CurrentState = new NormalState();
                        break;
                    default:
                        parser.CurrentPart += parser.CurrentCharacter;
                        break;
                }
                parser.Location++;
            }
        }

        private class InsideDoubleQuotes : IParserState
        {
            public void Process(Parser parser)
            {
                switch (parser.CurrentCharacter)
                {
                    case '"':
                        parser.CurrentState = new NormalState();
                        break;
                    default:
                        parser.CurrentPart += parser.CurrentCharacter;
                        break;
                }
                parser.Location++;
            }
        }

        public class ParserException : Exception
        {
            public ParserException(string message) : base(message)
            {
            }
        }
    }
}
