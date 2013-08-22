using System.Collections.Generic;

namespace ProtoCore.Lang
{
    public class FunctionTable
    {
        //@FIXME (luke): This shouldn't be public, the whole thing needs better refactoring to support
        //DI test uses
        public FunctionTable()
        {
        }

        public FunctionGroup GetFunctionGroup(int classIndex, string functionName)
        {
            if (functionName == null)
                throw new System.ArgumentNullException("functionName");

            Dictionary<string, FunctionGroup> funcGroupMap;
            if (!GlobalFuncTable.TryGetValue(classIndex, out funcGroupMap))
            {
                return null;
            }

            FunctionGroup funcGroup;
            if (!funcGroupMap.TryGetValue(functionName, out funcGroup))
            {
                return null;
            }

            return funcGroup;
        }

        public void AddFunctionEndPointer(int classIndex, string functionName, FunctionEndPoint fep)
        {
            if (functionName == null)
                throw new System.ArgumentNullException("functionName");

            if (fep == null)
                throw new System.ArgumentNullException("fep");

            Dictionary<string, FunctionGroup> funcGroupMap;
            if (!GlobalFuncTable.TryGetValue(classIndex, out funcGroupMap))
            {
                funcGroupMap = new Dictionary<string, FunctionGroup>();
                GlobalFuncTable[classIndex] = funcGroupMap;
            }

            FunctionGroup funcGroup;
            if (!funcGroupMap.TryGetValue(functionName, out funcGroup))
            {
                funcGroup = new FunctionGroup();
                funcGroupMap[functionName] = funcGroup;
            }

            funcGroup.FunctionEndPoints.Add(fep);
        }

        public void CopyVisibleFunctionEndPointFromBase(int classIndex, int baseClassIndex)
        {
            if (classIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                throw new System.ArgumentException("classIndex");
            }

            if (baseClassIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                throw new System.ArgumentException("classIndex");
            }

            Dictionary<string, FunctionGroup> thisFunctionGroupMap;
            Dictionary<string, FunctionGroup> baseFunctionGroupMap;

            if (!GlobalFuncTable.TryGetValue(classIndex, out thisFunctionGroupMap) ||
                !GlobalFuncTable.TryGetValue(baseClassIndex, out baseFunctionGroupMap))
            {
                return;
            }

            foreach (KeyValuePair<string, FunctionGroup> baseGroup in baseFunctionGroupMap)
            {
                FunctionGroup functionGroup;
                if (thisFunctionGroupMap.TryGetValue(baseGroup.Key, out functionGroup))
                {
                    functionGroup.CopyVisible(baseGroup.Value.FunctionEndPoints);
                }
                else
                {
                    // If this class doesnt have basegroup, create a new group and append the visible feps from the basegoup
                    functionGroup = new FunctionGroup();
                    functionGroup.CopyVisible(baseGroup.Value.FunctionEndPoints);
                    if (functionGroup.FunctionEndPoints.Count > 0)
                    {
                        thisFunctionGroupMap[baseGroup.Key] = functionGroup;
                    }
                }
            }
        }

        private Dictionary<string, FunctionGroup> FunctionList = new Dictionary<string,FunctionGroup>();
        private Dictionary<int, Dictionary<string, FunctionGroup> >GlobalFuncTable = new Dictionary<int,Dictionary<string,FunctionGroup>>();
    }
}
