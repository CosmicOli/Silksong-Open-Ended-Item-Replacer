# Silksong-Open-Ended-Item-Replacer
A mod designed to replace most 'checks' with an an open ended request for an item that other mods can respond to.  

This mod is not intended as a great or fully formalised item replacer; it is being made as the more generally accepted and definitely more thought out ItemChanger.Core has not been fully ported to Silksong yet, and I felt it would be fun to try code something together in my free time with a one week deadline.  

Hence though, while I will put some effort into compatibility and reasonable design practices, this project will not bloom into anything comparible to the much more standardised product that the ItemChanger port will soon become.  

Upon functionality being achieved, I likely will not continue to update this, however feel free to report bugs or make pull requests and I'll try have a look.  

Note about design: For the most part, items are replaced by moving the original into an inaccessible location (-250, -250) , removing all interactions, and creating a new child as a replacement  
-> This has some obvious jank to it as far as being a solution is concerned, but it for the most part doesn't effect functionality in a negative way  
-> There were some reasons I chose this:  
-> -> I was worried highjacking existing pickups may lead to unintended behaviour or player data changes outside of the SavedItem being gotten; I honestly am not sure if this ever actually is the case, especially as I have also disabled all original persistence  
-> -> Spawn conditions that are independant to persistence are easily preserved without manual patching; e.g. weighted belt only dropping as an item in act 3  
-> -> Original item physical behaviour is preserved; e.g. mask shard replacement pickups still sway up and down, and I think that's neat  
There are unfortunately some issues with this as an implementation, which may lead to a redesign later:  
-> Some physical behaviours become a bit buggy  
-> -> Craw feather replacements don't sway down  
-> -> Some CollectableItemPickup droppers, by nature of changing the location of the pickup they drop in post, create some very weird behaviour in cases that unfortunately do require manual patching  
-> -> -> There are only a few relevant cases that are all manually patched to not be an issue, but it will be a bigger issue if things like boss drops for quests get added as currently they have problems  
-> -> -> -> I mean, I could just manually patch them also, but I think a more robust solution that doesn't cause weird behaviour that needs manual fixing is probably better longterm  