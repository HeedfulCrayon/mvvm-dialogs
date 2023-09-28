﻿using System.Diagnostics;
using System.Windows;
using Moq;
using MvvmDialogs.Views;
using System.Linq;
using System.Collections.Generic;

namespace MvvmDialogs;

public class DialogServiceViewsTest : IDisposable
{
    public void Dispose() => DialogServiceViews.Clear();

    [StaFact]
    public void RegisterWindowUsingAttachedProperty()
    {
        // Arrange
        var window = new Window();

        var expected = new[]
        {
            new ViewWrapper(window)
        };

        // Act
        window.SetValue(DialogServiceViews.IsRegisteredProperty, true);
            
        // Assert
        Assert.Equal(expected, DialogServiceViews.Views);
    }

    [StaFact]
    public void UnregisterWindowUsingAttachedProperty()
    {
        // Arrange
        var window = new Window();
        window.SetValue(DialogServiceViews.IsRegisteredProperty, true);
            
        // Act
        window.SetValue(DialogServiceViews.IsRegisteredProperty, false);

        // Assert
        Assert.Empty(DialogServiceViews.Views);
    }

    [StaFact]
    public void RegisterFrameworkElementUsingAttachedProperty()
    {
        // Arrange
        var frameworkElement = new FrameworkElement();
            
        var unused = new Window
        {
            Content = frameworkElement
        };

        var expected = new[]
        {
            new ViewWrapper(frameworkElement)
        };

        // Act
        frameworkElement.SetValue(DialogServiceViews.IsRegisteredProperty, true);

        // Assert
        Assert.Equal(expected, DialogServiceViews.Views);
    }

    [StaFact]
    public void UnregisterFrameworkElementUsingAttachedProperty()
    {
        // Arrange
        var frameworkElement = new FrameworkElement();
            

        var unused = new Window
        {
            Content = frameworkElement
        };

        frameworkElement.SetValue(DialogServiceViews.IsRegisteredProperty, true);
            
        // Act
        frameworkElement.SetValue(DialogServiceViews.IsRegisteredProperty, false);

        // Assert
        Assert.Empty(DialogServiceViews.Views);
    }

    [StaFact]
    public void RegisterLoadedView()
    {
        // Arrange
        var view = new Mock<FrameworkElementMock>();
        view
            .Setup(mock => mock.IsAlive)
            .Returns(true);
        view
            .Setup(mock => mock.GetOwner())
            .Returns(new Window());

        var expected = new[]
        {
            view.Object
        };
            
        // Act
        DialogServiceViews.Register(view.Object);

        // Assert
        Assert.Equal(expected, DialogServiceViews.Views);
    }

    [StaFact]
    public void RegisterLoadedViewOnMultipleThreads()
    {
        // Arrange
        List<Thread> threads = new List<Thread>();
        int numberOfViews = 10;
        for (var i = 0; i < numberOfViews - 1; i++)
        {
            Thread newThread = new Thread(new ParameterizedThreadStart(CreateViewOnSeparateThreadHelper));
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
            threads.Add(newThread);
        }

        while (threads.Any(thread => thread.IsAlive))
        {
            Thread.Sleep(50);
        }

        var view = new Mock<FrameworkElementMock>();
        view
            .Setup(mock => mock.IsAlive)
            .Returns(true);
        view
            .Setup(mock => mock.GetOwner())
            .Returns(new Window());

        var expected = new[]
        {
            view.Object
        };

        // Act
        DialogServiceViews.Register(view.Object);

        // Assert
        // We expect the number of views registered to be higher than what is
        // returned via the DialogServiceViews.Views property since the property
        // only returns the views that are on the existing thread.
        Assert.Multiple(() =>
        {
            Assert.Equal(numberOfViews, DialogServiceViews.ViewCount);
            Assert.Equal(expected, DialogServiceViews.Views);
        });
    }

    [StaFact]
    public void UnregisterLoadedView()
    {
        // Arrange
        var view = new Mock<FrameworkElementMock>();
        view
            .Setup(mock => mock.IsAlive)
            .Returns(true);
        view
            .Setup(mock => mock.GetOwner())
            .Returns(new Window());

        DialogServiceViews.SetIsRegistered(view.Object, true);

        // Act
        DialogServiceViews.SetIsRegistered(view.Object, false);

        // Assert
        Assert.Empty(DialogServiceViews.Views);
    }

    [StaFact]
    public void RegisterViewThatNeverGetsLoaded()
    {
        // Arrange
        var view = new Mock<FrameworkElementMock>();
        view
            .Setup(mock => mock.IsAlive)
            .Returns(true);
            
        // Act
        DialogServiceViews.Register(view.Object);

        // Assert
        Assert.Empty(DialogServiceViews.Views);
    }

    [StaFact]
    public void RegisterViewThatGetsLoaded()
    {
        // Arrange
        var view = new Mock<FrameworkElementMock>();
        view
            .Setup(mock => mock.IsAlive)
            .Returns(true);
            
        // At time of register the view has no parent, thus is not loaded
        DialogServiceViews.Register(view.Object);

        // After register we can simulate that the view gets loaded
        view
            .Setup(mock => mock.GetOwner())
            .Returns(new Window());

        var expected = new[]
        {
            view.Object
        };

        // Act
        view.Raise(mock => mock.Loaded += null, new RoutedEventArgs(null, view.Object));

        // Assert
        Assert.Equal(expected, DialogServiceViews.Views);
    }

    [StaFact]
    public void UnregisterWhenClosingOwner()
    {
        // Arrange
        var window = new Window();

        var view = new Mock<FrameworkElementMock>();
        view
            .Setup(mock => mock.IsAlive)
            .Returns(true);
        view
            .Setup(mock => mock.GetOwner())
            .Returns(window);

        DialogServiceViews.Register(view.Object);

        // Act
        window.Close();
            
        // Assert
        Assert.Empty(DialogServiceViews.Views);
    }

    private void CreateViewOnSeparateThreadHelper(object data)
    {
        var view = new Mock<FrameworkElementMock>();
        view
            .Setup(mock => mock.IsAlive)
            .Returns(true);
        view
            .Setup(mock => mock.GetOwner())
            .Returns(new Window() { DataContext = "New Thread Window" });

        DialogServiceViews.Register(view.Object);
    }

    #region Helper classes

    // ReSharper disable once MemberCanBePrivate.Global
    public abstract class FrameworkElementMock : FrameworkElement, IView
    {
        public new abstract event RoutedEventHandler Loaded;

        public abstract int Id { get; }

        public abstract FrameworkElement Source { get; }

        public new abstract object DataContext { get; }

        public abstract bool IsAlive { get; }

        public abstract Window GetOwner();
    }

    #endregion
}
