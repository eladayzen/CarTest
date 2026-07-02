// Interface used by SpawnSystem and GridOptimisation
namespace TS.Generics
{
    public interface IValidateAction<T>
    {
        public void ValidateAction(T actionState);
    }
}
