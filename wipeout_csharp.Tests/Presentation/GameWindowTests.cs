using System;
using System.Collections.Generic;
using Moq;
using OpenTK.Windowing.Common;
using Xunit;

namespace WipeoutRewrite.Tests.Presentation
{
    /// <summary>
    /// Tests for IGameWindow interface and mocking patterns.
    /// Verifies that IGameWindow abstraction enables testing without OpenGL context.
    /// </summary>
    public class GameWindowTests
    {
        /// <summary>
        /// Verifies that IGameWindow can be mocked and basic properties are accessible.
        /// </summary>
        [Fact]
        public void IGameWindow_CanBeMocked_WithBasicProperties()
        {
            // Arrange: Create a mock IGameWindow
            var mockWindow = new Mock<IGameWindow>();
            mockWindow.Setup(w => w.Size).Returns(new OpenTK.Mathematics.Vector2i(1920, 1080));
            mockWindow.Setup(w => w.ClientSize).Returns(new OpenTK.Mathematics.Vector2i(1920, 1080));
            mockWindow.Setup(w => w.IsExiting).Returns(false);

            // Act
            var window = mockWindow.Object;

            // Assert
            Assert.NotNull(window);
            Assert.Equal(1920, window.Size.X);
            Assert.Equal(1080, window.Size.Y);
            Assert.Equal(1920, window.ClientSize.X);
            Assert.False(window.IsExiting);
        }

        /// <summary>
        /// Verifies that IGameWindow mock supports different window sizes.
        /// </summary>
        [Fact]
        public void IGameWindow_MockSupports_DifferentWindowSizes()
        {
            // Arrange
            var mockWindow = new Mock<IGameWindow>();
            mockWindow.Setup(w => w.ClientSize).Returns(new OpenTK.Mathematics.Vector2i(1024, 768));
            mockWindow.Setup(w => w.IsExiting).Returns(false);

            // Act
            var window = mockWindow.Object;
            
            // Assert
            Assert.NotNull(window);
            Assert.Equal(1024, window.ClientSize.X);
            Assert.Equal(768, window.ClientSize.Y);
        }

        /// <summary>
        /// Verifies that UpdateFrame event can be subscribed to without errors.
        /// </summary>
        [Fact]
        public void IGameWindow_UpdateFrameEvent_CanBeSubscribed()
        {
            // Arrange
            var mockWindow = new Mock<IGameWindow>();
            var stateHistory = new List<string>();

            mockWindow.Setup(w => w.IsExiting).Returns(false);
            mockWindow.SetupAdd(w => w.UpdateFrame += It.IsAny<Action<FrameEventArgs>>());

            // Act: Subscribe to event
            var eventCalled = false;
            mockWindow.Object.UpdateFrame += e => 
            {
                eventCalled = true;
                stateHistory.Add("UpdateFrame");
            };

            // Assert: Event subscription worked
            Assert.NotNull(mockWindow.Object);
            mockWindow.VerifyAdd(w => w.UpdateFrame += It.IsAny<Action<FrameEventArgs>>(), Times.Once);
        }

        /// <summary>
        /// Verifies that Resize event can be subscribed to and mock is accessible.
        /// </summary>
        [Fact]
        public void IGameWindow_ResizeEvent_CanBeSubscribed()
        {
            // Arrange
            var mockWindow = new Mock<IGameWindow>();
            mockWindow.Setup(w => w.ClientSize).Returns(new OpenTK.Mathematics.Vector2i(1280, 720));
            mockWindow.SetupAdd(w => w.Resize += It.IsAny<Action<ResizeEventArgs>>());

            // Act: Subscribe to resize event
            var resizeCalled = false;
            mockWindow.Object.Resize += args => { resizeCalled = true; };

            // Assert
            Assert.NotNull(mockWindow.Object);
            mockWindow.VerifyAdd(w => w.Resize += It.IsAny<Action<ResizeEventArgs>>(), Times.Once);
        }

        /// <summary>
        /// Verifies that all lifecycle events can be subscribed to.
        /// </summary>
        [Fact]
        public void IGameWindow_LifecycleEvents_CanAllBeSubscribed()
        {
            // Arrange
            var mockWindow = new Mock<IGameWindow>();
            var lifecycleEvents = new List<string>();

            // Setup all lifecycle events
            mockWindow.SetupAdd(w => w.Load += It.IsAny<Action>());
            mockWindow.SetupAdd(w => w.UpdateFrame += It.IsAny<Action<FrameEventArgs>>());
            mockWindow.SetupAdd(w => w.RenderFrame += It.IsAny<Action<FrameEventArgs>>());
            mockWindow.SetupAdd(w => w.Unload += It.IsAny<Action>());

            // Act: Subscribe to all lifecycle events
            mockWindow.Object.Load += () => lifecycleEvents.Add("Load");
            mockWindow.Object.UpdateFrame += e => lifecycleEvents.Add("UpdateFrame");
            mockWindow.Object.RenderFrame += e => lifecycleEvents.Add("RenderFrame");
            mockWindow.Object.Unload += () => lifecycleEvents.Add("Unload");

            // Assert: All events subscribed successfully
            Assert.NotNull(mockWindow.Object);
            mockWindow.VerifyAdd(w => w.Load += It.IsAny<Action>(), Times.Once);
            mockWindow.VerifyAdd(w => w.UpdateFrame += It.IsAny<Action<FrameEventArgs>>(), Times.Once);
            mockWindow.VerifyAdd(w => w.RenderFrame += It.IsAny<Action<FrameEventArgs>>(), Times.Once);
            mockWindow.VerifyAdd(w => w.Unload += It.IsAny<Action>(), Times.Once);
        }

        /// <summary>
        /// Verifies that IGameWindow can be mocked without requiring OpenGL initialization.
        /// </summary>
        [Fact]
        public void IGameWindow_CanBeMocked_WithoutOpenGLContext()
        {
            // Arrange: No SDL required, no graphics context needed
            var mockWindow = new Mock<IGameWindow>();
            mockWindow.Setup(w => w.Size).Returns(new OpenTK.Mathematics.Vector2i(800, 600));

            // Act: Access window without OpenGL
            var window = mockWindow.Object;

            // Assert: Mock works without platform dependencies
            Assert.NotNull(window);
            Assert.Equal(800, window.Size.X);
            Assert.Equal(600, window.Size.Y);
        }
    }
}
