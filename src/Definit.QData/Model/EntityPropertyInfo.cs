namespace Definit.QData.Model
{
    public class EntityPropertyInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public QDataPropertyRoles PropertyRole { get; set; }
        //public QDataValueAutoGenerationTypes ValueAutoGeneration { get; set; }
    }

}