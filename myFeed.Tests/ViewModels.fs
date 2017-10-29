namespace myFeed.Tests.ViewModels

open Xunit    
open NSubstitute

open System
open System.Threading.Tasks

open myFeed.Tests.Extensions

open myFeed.ViewModels.Bindables
open myFeed.ViewModels.Implementations

open myFeed.Tests.Extensions.Domain
open myFeed.Tests.Extensions.Dependency

open myFeed.Repositories.Abstractions
open myFeed.Repositories.Models

open myFeed.Services.Abstractions

module ObservablePropertyFixture =

    [<Fact>] 
    let ``should raise property change event on value change``() =

        let mutable fired = 0
        let property = ObservableProperty(42)
        property.PropertyChanged += fun _ -> fired <- fired + 1
        property.Value <- 3
        Should.equal 1 fired 

    [<Fact>]
    let ``should not fire property canged event if value is the same``() =
    
        let mutable fired = 0
        let property = ObservableProperty(42)
        property.PropertyChanged += fun _ -> fired <- fired + 1
        property.Value <- 42
        Should.equal 0 fired

    [<Fact>]
    let ``should treat property name as value string``() =

        let property = ObservableProperty(42)
        property.PropertyChanged += fun e -> Should.equal "Value" e.PropertyName
        property.Value <- 3    
        
    [<Fact>]
    let ``should slowly initialize value via funtion returning task``() =

        let property = ObservableProperty<string>(fun () -> "Foo" |> Task.FromResult)
        Should.equal "Foo" property.Value    

module ObservableCommandFixture =

    [<Fact>]
    let ``should execute passed actions``() =

        let mutable fired = 0
        let command = Func<Task>(fun () -> 
            fired <- fired + 1 
            Task.CompletedTask) |> ObservableCommand
        command.Execute()
        Should.equal true (command.CanExecute()) 
        Should.equal 1 fired

    [<Fact>]
    let ``should await previous execution``() =

        let command = Func<Task>(fun () -> Task.Delay(1000)) |> ObservableCommand
        command.Execute()
        Should.equal false (command.CanExecute())

    [<Fact>]
    let ``should raise state change event``() =

        let mutable fired = 0
        let command = Func<Task>(fun () -> Task.CompletedTask) |> ObservableCommand
        command.CanExecuteChanged += fun _ -> fired <- fired + 1
        command.Execute()
        Should.equal 2 fired 

module FaveViewModelFixture =

    [<Fact>]
    let ``should populate view model with items received from repository``() =   

        let favorites = Substitute.For<IFavoritesRepository>()
        favorites.GetAllAsync().Returns([ Article(Title="Foo") ] 
            :> seq<_> |> Task.FromResult) |> ignore

        let factory = Substitute.For<IFactoryService>()    
        factory.CreateInstance<ArticleViewModel>(Arg.Any()).Returns(fun x -> 
            [x.Arg<obj[]>().[0]] |> produce<ArticleViewModel>) |> ignore

        let faveViewModel = produce<FaveViewModel> [favorites; factory]
        faveViewModel.Load.CanExecuteChanged += fun _ -> 
            if faveViewModel.Load.CanExecute() then

                Should.equal 1 faveViewModel.Items.Count
                Should.equal "Foo" faveViewModel.Items.[0].Title.Value 
                
        faveViewModel.Load.Execute()

    [<Fact>]
    let ``should order items in view model by name descending``() =    
        
        let favorites = Substitute.For<IFavoritesRepository>()
        favorites.GetAllAsync().Returns(
            [ Article(Title="C"); Article(Title="A"); Article(Title="B"); ] 
            :> seq<_> |> Task.FromResult) |> ignore
            
        let factory = Substitute.For<IFactoryService>()    
        factory.CreateInstance<ArticleViewModel>(Arg.Any()).Returns(fun x -> 
            [x.Arg<obj[]>().[0]] |> produce<ArticleViewModel>) |> ignore

        let faveViewModel = produce<FaveViewModel> [favorites; factory]
        faveViewModel.Load.CanExecuteChanged += fun _ ->
            if faveViewModel.Load.CanExecute() then
                faveViewModel.OrderByName.CanExecuteChanged += fun _ -> 
                    if faveViewModel.OrderByName.CanExecute() then

                        Should.equal 3 faveViewModel.Items.Count
                        Should.equal "A" faveViewModel.Items.[0].Title.Value
                        Should.equal "B" faveViewModel.Items.[1].Title.Value
                        Should.equal "C" faveViewModel.Items.[2].Title.Value

                faveViewModel.OrderByName.Execute() 
        faveViewModel.Load.Execute() 
              
    [<Fact>]
    let ``should order items in view model by date descending``() =    
        
        let favorites = Substitute.For<IFavoritesRepository>()
        favorites.GetAllAsync().Returns(
            [ Article(Title="A", PublishedDate=DateTime.MaxValue); 
              Article(Title="C", PublishedDate=DateTime.MinValue); 
              Article(Title="B", PublishedDate=DateTime.Now); ] 
            :> seq<_> |> Task.FromResult) |> ignore
            
        let factory = Substitute.For<IFactoryService>()    
        factory.CreateInstance<ArticleViewModel>(Arg.Any()).Returns(fun x -> 
            [x.Arg<obj[]>().[0]] |> produce<ArticleViewModel>) |> ignore

        let faveViewModel = produce<FaveViewModel> [favorites; factory]
        faveViewModel.Load.CanExecuteChanged += fun _ ->
            if faveViewModel.Load.CanExecute() then
                faveViewModel.OrderByDate.CanExecuteChanged += fun _ -> 
                    if faveViewModel.OrderByDate.CanExecute() then

                        Should.equal 3 faveViewModel.Items.Count
                        Should.equal "A" faveViewModel.Items.[0].Title.Value
                        Should.equal "B" faveViewModel.Items.[1].Title.Value
                        Should.equal "C" faveViewModel.Items.[2].Title.Value

                faveViewModel.OrderByDate.Execute() 
        faveViewModel.Load.Execute()       