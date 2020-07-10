using X.Homylogic.Models.Objects;

namespace HomylogicAsp.Models.Variables
{
    public class EditVariableViewModel : EditObjectViewModel
    {
        public VariableX.VariableTypes VariableType { get; set; }
        // public bool ShowOnHome { get; set; } * Not used now, prepared for future use ...
    }
}
