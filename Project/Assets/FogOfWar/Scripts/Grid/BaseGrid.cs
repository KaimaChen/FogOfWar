
public class BaseGrid
{
    protected int m_width;
    protected int m_height;

    public BaseGrid(int w, int h)
    {
        m_width = w;
        m_height = h;
    }

    protected int Index(int x, int y)
    {
        return x + y * m_width;
    }
}