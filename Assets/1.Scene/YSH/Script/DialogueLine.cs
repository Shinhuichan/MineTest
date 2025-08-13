using System.Collections.Generic;

[System.Serializable]
public class DialogueLine
{
    public string ID;
    public string ParentID;
    public string Character;
    public string ContentText; // Content 대신 ContentText로 변경
    public string NextID;
    public List<string> Choices;
}