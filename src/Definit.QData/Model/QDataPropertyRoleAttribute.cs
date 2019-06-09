using System;

namespace Definit.QData.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public class QDataPropertyRoleAttribute : Attribute
    {
        private QDataPropertyRoles _cpType;

        public QDataPropertyRoleAttribute(QDataPropertyRoles cpType)
        {
            _cpType = cpType;
        }

        public QDataPropertyRoles QDataPropertyRole { get { return _cpType; } }
    }

}