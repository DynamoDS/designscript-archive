using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DesignScript.Editor.Core
{
    public class FunctionCallPart
    {
        private System.Drawing.Point startPoint;
        private System.Drawing.Point endPoint;
        private List<FunctionCallPart> argumentParts = null;

        public FunctionCallPart()
        {
            this.Identifier = string.Empty;
            this.startPoint = new System.Drawing.Point(-1, -1);
            this.endPoint = new System.Drawing.Point(-1, -1);
        }

        public void SetStartPoint(FunctionCallParser.Token token, bool includeToken)
        {
            System.Drawing.Point temporary = new System.Drawing.Point(token.col - 1, token.line - 1);

            if (false == includeToken)
            {
                temporary.X = token.col + token.val.Length - 1;
                temporary.Y = token.line - 1;
            }

            SetStartPoint(temporary);
        }

        public void SetStartPoint(System.Drawing.Point point)
        {
            UpdateStartPointInternal(point);
        }

        public void SetEndPoint(FunctionCallParser.Token token, bool includeToken)
        {
            System.Drawing.Point temporary = new System.Drawing.Point(token.col - 1, token.line - 1);

            if (false == includeToken)
            {
                temporary.X = token.col + token.val.Length - 1;
                temporary.Y = token.line - 1;
            }

            SetEndPoint(temporary);
        }

        public void SetEndPoint(System.Drawing.Point point)
        {
            UpdateEndPointInternal(point);
        }

        public void AppendIdentifier(FunctionCallParser.Token token)
        {
            if (null != token && (!string.IsNullOrEmpty(token.val)))
                this.Identifier = this.Identifier + token.val;
        }

        public void AddDefaultArgument(System.Drawing.Point start, System.Drawing.Point end)
        {
            // This "default argument" is not the same as a term used in regular 
            // context. It is an argument for function call that does not specify
            // any argument. This is inserted so that when queried, the argument 
            // index at the cursor location returns "0" instead of "-1" (which 
            // means there's no argument found in cursor location). AutoComplete 
            // tooltip should always highlight the first argument even before the
            // first argument is typed in. This should only be done when there is
            // no argument specified yet (as user types).
            // 
            System.Diagnostics.Debug.Assert(this.HasArgument == false);

            FunctionCallPart defaultArgument = new FunctionCallPart();
            defaultArgument.SetStartPoint(start);
            defaultArgument.SetEndPoint(end);
            this.AddArgumentPart(defaultArgument);
        }

        public void AddArgumentPart(FunctionCallPart argumentPart)
        {
            if (null == argumentParts)
                argumentParts = new List<FunctionCallPart>();

            argumentPart.ParentPart = this;
            argumentParts.Add(argumentPart);

            UpdateStartPointInternal(argumentPart.StartPoint);
            UpdateEndPointInternal(argumentPart.EndPoint);
        }

        public int GetArgumentIndex(FunctionCallPart argumentPart)
        {
            if (null == argumentParts)
                return -1;

            return argumentParts.IndexOf(argumentPart);
        }

        public FunctionCallPart GetIntersectionPart(int column, int line)
        {
            System.Drawing.Point point = new System.Drawing.Point(column, line);
            return (GetIntersectionPart(point));
        }

        public FunctionCallPart GetIntersectionPart(System.Drawing.Point point)
        {
            if (null != argumentParts)
            {
                foreach (FunctionCallPart part in argumentParts)
                {
                    FunctionCallPart intersection = part.GetIntersectionPart(point);
                    if (null != intersection)
                        return intersection;
                }
            }

            // If there was no child argument parts, or none of them is intersecting 
            // with the input point, then test the parent (this) function call part.

            if (point.Y < startPoint.Y || (point.Y > endPoint.Y))
                return null; // The point lies outside of the range.
            if (point.Y == startPoint.Y && (point.X < startPoint.X))
                return null; // On the first line, but outside of range.
            if (point.Y == endPoint.Y && (point.X > endPoint.X))
                return null; // On the last line, but outside of range.

            return this;
        }

        public bool HasValidRange
        {
            get
            {
                return ((startPoint.X != -1) && (endPoint.X != -1) &&
                        (startPoint.Y != -1) && (endPoint.Y != -1));
            }
        }

        public bool HasArgument
        {
            get
            {
                // This property indicates if this function part has any argument.
                return (null != argumentParts && (argumentParts.Count > 0));
            }
        }

        public string Identifier { get; set; }
        public FunctionCallPart ParentPart { get; set; }
        public System.Drawing.Point StartPoint { get { return this.startPoint; } }
        public System.Drawing.Point EndPoint { get { return this.endPoint; } }

        private void UpdateStartPointInternal(System.Drawing.Point point)
        {
            if (-1 == startPoint.X || (-1 == startPoint.Y))
                startPoint = point;
            else if (-1 != point.X && (-1 != point.Y))
            {
                if (startPoint.Y > point.Y)
                    startPoint = point;
                else if (startPoint.Y == point.Y)
                {
                    if (startPoint.X > point.X)
                        startPoint.X = point.X;
                }
            }
        }

        private void UpdateEndPointInternal(System.Drawing.Point point)
        {
            if (-1 == endPoint.X || (-1 == endPoint.Y))
                endPoint = point;
            else if (-1 != point.X && (-1 != point.Y))
            {
                if (endPoint.Y < point.Y)
                    endPoint = point;
                else if (endPoint.Y == point.Y)
                {
                    if (endPoint.X < point.X)
                        endPoint.X = point.X;
                }
            }
        }
    }

    public class FunctionCallContext
    {
        int lineOffsetIntoContent = 0;
        FunctionCallPart topMostFunctionPart = null;

        #region Static Helper Methods

        public static FunctionCallContext Build(List<string> content, System.Drawing.Point position)
        {
            if (null == content || (content.Count <= 0))
                return null;
            if (position.Y < 0 || (position.Y >= content.Count))
                return null;

            string line = content[position.Y];
            if (string.IsNullOrEmpty(line))
                return null;

            // Note that "position.X" is allowed to be one character after the 
            // "line" (but not more than that). This is because before indexing 
            // into "line", "position.X" is decremented by one first, making it 
            // fall within the range of valid indices.
            if (position.X < 0 || (position.X > line.Length))
                return null;

            System.Drawing.Point startPoint = new System.Drawing.Point();
            int openBrackets = ScanBackwardOpenBrackets(content, position, ref startPoint);
            if (0 == openBrackets)
                return null;

            System.Drawing.Point endPoint = new System.Drawing.Point();
            ScanForwardEndPoint(content, startPoint, ref endPoint);

            return new FunctionCallContext(content, startPoint, endPoint);
        }

        private static int ScanBackwardOpenBrackets(List<string> content,
            System.Drawing.Point position, ref System.Drawing.Point start)
        {
            int openBrackets = 0;
            System.Drawing.Point current = position;

            // Scan backward for terminating characters.
            while (true)
            {
                switch (GetPrevious(content, ref current))
                {
                    case '(':
                        openBrackets++;
                        break;

                    case ';':
                    case '=':
                        // If we have found any open bracket, then 
                        // this is within a possible function call.
                        start = current;
                        start.X = start.X + 1;
                        return openBrackets;

                    case char.MinValue:
                        return openBrackets;
                }
            }
        }

        private static void ScanForwardEndPoint(List<string> content,
            System.Drawing.Point start, ref System.Drawing.Point end)
        {
            int openBrackets = 0;

            end = start;
            System.Drawing.Point current = start;
            string line = content[current.Y];

            while (true)
            {
                end = current;
                switch (GetNext(content, ref current))
                {
                    case '(': openBrackets++; break;
                    case ')':
                        openBrackets--;
                        if (0 == openBrackets)
                        {
                            end = current;
                            return;
                        }
                        break;

                    case ';':
                    case '=':
                        end.X = end.X - 1;
                        if (end.X < 0)
                            end.X = 0;
                        return;

                    case char.MinValue:
                        return;
                }
            }
        }

        private static char GetNext(List<string> content, ref System.Drawing.Point position)
        {
            position.X = position.X + 1;

            string line = content[position.Y];
            if (position.X >= line.Length)
            {
                while (position.Y < content.Count)
                {
                    position.Y = position.Y + 1;
                    if (position.Y >= content.Count)
                        return char.MinValue;
                    if (!string.IsNullOrEmpty(content[position.Y]))
                        break;
                }

                line = content[position.Y];
                position.X = 0;
            }

            return line[position.X];
        }

        private static char GetPrevious(List<string> content, ref System.Drawing.Point position)
        {
            position.X = position.X - 1;
            string line = content[position.Y];

            if (position.X < 0)
            {
                while (position.Y >= 0)
                {
                    position.Y = position.Y - 1;
                    if (position.Y < 0)
                        return char.MinValue;
                    if (!string.IsNullOrEmpty(content[position.Y]))
                        break; // Found a non-empty line.
                }

                line = content[position.Y];
                position.X = line.Length - 1;
            }

            return line[position.X];
        }

        #endregion

        #region Public Class Methods and Properties

        public string GetFunctionAtPoint(int column, int line, out int argumentIndex)
        {
            // Translate global line index to local index.
            line = line - lineOffsetIntoContent;

            argumentIndex = -1; // No argument.
            if (null == topMostFunctionPart)
                return null;

            FunctionCallPart intersection = topMostFunctionPart.GetIntersectionPart(column, line);
            if (null == intersection)
                return null;

            FunctionCallPart parentPart = intersection.ParentPart as FunctionCallPart;
            if (null != parentPart)
            {
                argumentIndex = parentPart.GetArgumentIndex(intersection);
                return parentPart.Identifier;
            }

            System.Diagnostics.Debug.Assert(intersection == topMostFunctionPart);
            if (intersection == topMostFunctionPart)
            {
                argumentIndex = 0;
                return topMostFunctionPart.Identifier;
            }

            return null;
        }

        public bool IsValidCallContext
        {
            get
            {
                if (null == topMostFunctionPart)
                    return false;
                if (string.IsNullOrEmpty(topMostFunctionPart.Identifier))
                    return false;

                return topMostFunctionPart.HasValidRange;
            }
        }

        #endregion

        private FunctionCallContext(List<string> content,
            System.Drawing.Point startPoint, System.Drawing.Point endPoint)
        {
            lineOffsetIntoContent = startPoint.Y;

            StringBuilder contentBuilder = new StringBuilder();
            for (int index = startPoint.Y; index <= endPoint.Y; ++index)
            {
                string line = content[index];
                if (index == startPoint.Y)
                {
                    // We're looking at the first line to copy, replace 
                    // everything before the start point with blank space.
                    if (startPoint.X > 0)
                        contentBuilder.Append(new string(' ', startPoint.X));

                    contentBuilder.Append(line.Substring(startPoint.X));
                    continue;
                }

                // We have come into region of another statement
                if (line.IndexOf('=') != -1)
                    break; // Get outta here!

                contentBuilder.Append(line);
            }

            string intermediate = contentBuilder.ToString();
            intermediate = PatchContent(intermediate);

            MemoryStream inputStream = new MemoryStream(
                Encoding.Default.GetBytes(intermediate));

            FunctionCallParser.Scanner scanner = new FunctionCallParser.Scanner(inputStream);
            FunctionCallParser.Parser parser = new FunctionCallParser.Parser(scanner);
            parser.Parse();
            topMostFunctionPart = parser.RootFunctionCallPart;
        }

        private string PatchContent(string intermediate)
        {
            string firstLine = intermediate;
            string remainingLines = string.Empty;

            int firstLineBreak = intermediate.IndexOf('\n');
            if (-1 != firstLineBreak)
            {
                firstLine = intermediate.Substring(0, firstLineBreak);
                remainingLines = intermediate.Substring(firstLineBreak);
            }

            StringBuilder builder = new StringBuilder();

            int equalSignIndex = firstLine.IndexOf('=');
            if (-1 == equalSignIndex)
                builder.Append(firstLine); // There's no equal sign.
            else
            {
                // Turn everything before the equal sign into space.
                builder.Append(new string(' ', equalSignIndex + 1));
                builder.Append(firstLine.Substring(equalSignIndex + 1));
            }

            builder.Append(remainingLines);
            return builder.ToString();
        }
    }
}
