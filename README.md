# Unity2D\_AID\_BastienMedinaGD2



**I/Concept**



Les différents jeux :



Un jeu mobile type arcade avec plusieurs levels composant tous une phase de jeu. Le joueur incarne un responsable de salle d'arcade ayant la capacité de se transformer en personnage de jeu d'arcade et en récupérant aussi leurs capacités. (ex : Le personnage peut se transformer en Donkey Kong et lancer des tonneaux). Le jeu se compose de 3 mini-jeux :



\-Game-and-Watch : Boss survivor Gameplay Le joueur doit esquiver des lasers qui spawnent selon des patterns définis aléatoirement au début. Les déplacements se font sur une grille de 5x5 cases et à chaque déplacement, le jeu s'update ce qui fait que le joueur doit prévoir ses déplacements en fonction de la position des lasers. Conditions Win/Loose : Win : À chaque apparition de lasers, une pièce spawn (elle spawn sur une case accessible par le joueur aléatoirement à au moins une case du joueur et une case d'une autre pièce. Les pièces disparaissent tous les 2 tours). Le joueur remporte le jeu s'il accumule 3 pièces. Loose : Le joueur perd la partie lorsqu'il n'a plus de vie. Il perd de la vie à chaque fois qu'il se fait toucher par un laser.



\-Arcade complex : Bullet Hell Gameplay Le joueur va devoir parcourir 4 étages de bureau délabrés et qui ressemblent aux liminal spaces dans le sens où il y a un grand open space composé de plein d'espaces de travail et quelques salles type salle de réunion, toilettes, salle de repos, etc. Le but de ce jeu est de récupérer un maximum d'équipement en fouillant la zone. Pour passer d'un niveau à l'autre, le joueur a un ascenseur qui est accessible à l'autre bout de l'open space. De plus, dans chaque pièce il y a entre 2 et 6 ennemis qui vont attaquer le joueur de différentes manières. Il peut également y avoir des ennemis cachés dans les objets fouillables, si c'est le cas, l'ennemi bondit pour attaquer le joueur. Pour se défendre, le joueur aura plusieurs armes qu'il sélectionnera avant l'entrée dans le donjon. Ces armes sont des capacités de personnages de jeu d'arcade connus de l'époque. Équipements disponibles si le joueur trouve des cartouches lorsqu'il fouille dans la zone de loot, ces dernières lui débloquent une nouvelle capacité de personnage. Conditions Win/Loose : Win : Si le joueur arrive à la fin des 3 couloirs, il a gagné. Loose : Si le joueur meurt durant le level, il perd. Il peut perdre de la vie s'il est touché par une attaque.



\-Arcade simple : Space invader Gameplay Le joueur doit détruire des virus informatiques qui lui foncent dessus pour survivre. Plus le temps passe et plus il en détruit, une jauge se remplit traduisant l'avancée dans le jeu. Plus cette jauge avance, plus les virus informatiques spawnent vite et en nombre. Pour se défendre de ces virus, le joueur tire un rayon de données informatiques qui détruit les virus informatiques. Conditions Win/Loose : Win : Le joueur remporte le jeu lorsque la barre de progression est arrivée au maximum. Loose : Si un des virus réussit à atteindre l'objet que défend le joueur, le jeu s'arrête et le joueur a perdu.



Organisation :

Tous ces mini-jeux s'imbriquent pour former un donjon se découpant en différentes phases. Chacune des phases est un mini-jeu. Dans un premier temps, le joueur monte d'étage en étage en fouillant pour s'équiper. À chaque étage monté, quand le joueur entre dans l'ascenseur, un panneau d'amélioration du personnage apparaît qui lui fait choisir aléatoirement entre plusieurs options comme vitesse, vie, max vie, gain d'objet, etc. Il n'y a pas de monnaie mais chaque objet que le joueur peut looter est associé à un indice de rareté allant de 1 à 3. Une fois que le joueur est monté de 3 étages, il peut prendre encore l'ascenseur qui le mène à l'étage 4 où il y a le combat de boss (Jeu Game and Watch). Si le joueur parvient à détruire le boss, il va se rendre compte que ce dernier cachait l'ascenseur qui mène à l'étage 5. À l'étage 5 il n'y a qu'une espèce de grand tube de verre au centre de la pièce. Ce tube est rempli de sable de cuivre et de circuits imprimés plongés dans un liquide lumineux qui pourrait s'apparenter à du formol. Juste devant le tube il y a une espèce de borne d'arcade qui permet au joueur s'il interagit avec de rentrer dans le système et de le combattre (jeu arcade space invader).





II/Narration



Context : Dans un futur proche, la terre est menacée par une IA extrêmement développée. Cette IA s'est développée à cause d'un humain qui a fait une demande un peu trop légère menant cette machine à détruire tout détracteur à son optimisation. Et la prochaine étape, c'est les humains.

Qui on joue : Mais alors qu'il ne semble rester plus aucun espoir, un personnage se démarque. Un ancien joueur professionnel de Dungeon's Lair (jeu d'arcade très populaire dans les années 80 qui consiste à sauver une princesse d'un donjon gardé par un terrible dragon.) Il se charge donc d'une mission aux yeux du monde : débrancher cette satanée machine avant qu'elle ne se mette à détruire les humains pour l'optimisation de sa tâche.

Pourquoi c'est lui qui doit aller sauver le monde ? Étant un ancien joueur pro d'un jeu aujourd'hui disparu, il possède des capacités spéciales dans l'avancée de donjon. De plus, le jeu ayant disparu, l'IA n'a pas d'information sur son fonctionnement et donc aucune info sur les stratégies que peut adopter le héros.

Son objectif : Il va donc devoir monter au 5ème étage de la tour pour combattre l'IA et la débrancher.





III/Visuel et artistique

Descriptions : Le style graphique du jeu est simple et épuré. Il est en vue 2D plate comme l'étaient les bornes d'arcade de l'époque. Le style est en pixel art spécifique. C'est-à-dire que les environnements et décors sont plutôt simples et épurés. Les couleurs sont très peu présentes avec aplat gris, blanc et noir appuyant encore plus l'environnement de travail morose et trop sérieux. Les couleurs de ce monde viennent des personnages et des virus informatiques. Tous les ennemis sont des virus, ou des créatures technologiques créées pour anéantir l'humanité. Au niveau des environnements, l'intention est dirigée vers de grands espaces de travail découpés en petits compartiments formant un open space. Le but est de se rapprocher d'une spatialisation proche des liminal spaces. Le but est également de montrer un espace normalement organisé et rangé dans des conditions de chaos.



