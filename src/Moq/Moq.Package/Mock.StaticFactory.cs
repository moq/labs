namespace Moq
{
    partial class Mock
    {
        static Mock()
        {
            MockFactory.Default = new Sdk.StaticMockFactory();
            OnInitialized();
        }

        /// <summary>
        /// Invoked after the default <see cref="MockFactory.Default"/> 
        /// is initialized.
        /// </summary>
        static partial void OnInitialized();
    }
}