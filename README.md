```mermaid
classDiagram
    class IConsumer {
        <<interface>>
    }
    class GeneratePageIllustrationConsumer {
    }
    IConsumer <|-- GeneratePageIllustrationConsumer
    class GeneratePageIllustrationConsumerDefinition {
    }
    ConsumerDefinition<GeneratePageIllustrationConsumer> <|-- GeneratePageIllustrationConsumerDefinition
    class IConsumer {
        <<interface>>
    }
    class GenerateStoryConsumer {
    }
    IConsumer <|-- GenerateStoryConsumer
    class GenerateStoryConsumerConsumerDefinition {
    }
    ConsumerDefinition<GenerateStoryConsumer> <|-- GenerateStoryConsumerConsumerDefinition
    class Story {
        +Title : string
        +Content : string
    }
    class StoryBook {
        +Title : string
        +Description : string
        +Pages : List<Page>
    }
    class Page {
        +ImageDescription : string
        +Content : string
        +PageNumber : int
    }
    class StoryEnpoints {
    }
    class IProjectMetadata {
        <<interface>>
    }
    class ChildrenStoryGenerator_API {
        +ProjectPath : string
    }
    IProjectMetadata <|-- ChildrenStoryGenerator_API
    class ChildrenStoryGenerator_AppHost {
        +ProjectPath : string
    }
    class Extensions {
    }
```
