namespace Moq
{
    internal partial class Mock
    {
        static Mock()
        {
            Sdk.MockFactory.Default = new Sdk.StaticMockFactory();
            OnInitialized();
        }

        /// <summary>
        /// Invoked after the default <see cref="MockFactory.Default"/> 
        /// is initialized.
        /// </summary>
        static partial void OnInitialized();
    }
}