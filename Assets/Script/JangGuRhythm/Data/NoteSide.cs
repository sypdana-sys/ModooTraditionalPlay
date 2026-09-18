namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 장구의 어느 쪽을 쳐야 하는지. Left = 북편(고, 왼손, 원), Right = 채편(편, 오른손, 직사각형),
    /// Cross = 넘겨치기(오른쪽 스폰 위치에서 원(Left) 모양으로 나오지만 왼손/O키로 쳐야 하는 노트).
    /// </summary>
    public enum NoteSide
    {
        Left,
        Right,
        Cross,
    }
}
