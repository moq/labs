namespace Moq
{
    partial class Mock
    {
        static Mock()
        {
            Sdk.MockFactory.Default = new Sdk.DynamicMockFactory();
            OnInitialized();
        }

        /// <summary>
        /// Invoked after the default <see cref="MockFactory.Default"/> 
        /// is initialized.
        /// </summary>
        static partial void OnInitialized();
    }
}