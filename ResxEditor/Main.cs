namespace ResxEditor
{
    public class Start
    {
        [STAThread]
        public static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new ResxEditor());
        }
    }
}