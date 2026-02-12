<template>
  <header class="w-full bg-white border-b border-slate-200 shadow-sm">
    <div class="max-w-6xl mx-auto px-4 h-20 flex items-center justify-between">
      
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 border-2 border-blue-600 rounded-full flex items-center justify-center overflow-hidden bg-white shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-7 h-7 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 10-3.84 4.799l1.156 3.47a1 1 0 001.907-.62a9 9 0 115.145-6.183l1.036 1.036a2 2 0 002.828 0l.022-.022z" />
          </svg>
        </div>
        
        <div class="flex flex-col">
          <h1 class="text-xl md:text-2xl font-black tracking-tighter text-slate-800 uppercase italic leading-none">
            Santé Scan
          </h1>
          <span class="text-[10px] uppercase tracking-[0.2em] text-blue-500 font-bold">Information Médicale</span>
        </div>
      </div>

      <nav class="hidden md:flex items-center gap-8">
        <a href="#" class="text-sm font-bold text-blue-600 border-b-2 border-blue-600 pb-1">Accueil</a>
        <a href="#" class="text-sm font-bold text-slate-400 hover:text-slate-600 transition-colors">Historique</a>
        <a href="#" class="text-sm font-bold text-slate-400 hover:text-slate-600 transition-colors">Conseils</a>
      </nav>

      <div class="flex items-center gap-2 cursor-pointer group">
        <div class="hidden sm:block text-right">
          <p class="text-xs font-bold text-slate-800 leading-none">Jeyapriya</p>
          <p class="text-[10px] text-slate-400">Mon Profil</p>
        </div>
        <div class="w-10 h-10 bg-slate-100 rounded-full border border-slate-200 flex items-center justify-center group-hover:bg-blue-50 transition-colors">
          <span class="text-lg">👤</span>
        </div>
      </div>

    </div>
  </header>
</template>

<script setup>
// Pas de logique complexe nécessaire ici pour le moment
</script>