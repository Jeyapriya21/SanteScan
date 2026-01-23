<template>
  <div class="min-h-screen bg-slate-200 flex justify-center items-center p-4">
    <div class="w-[380px] h-[780px] bg-white rounded-[3rem] shadow-2xl border-[10px] border-slate-900 overflow-hidden flex flex-col relative">
      
      <HomeView />

      <nav class="bg-gray-50 border-t flex justify-around p-4 pb-8 text-gray-400">
        <button class="text-blue-600 flex flex-col items-center">
          <i class="fas fa-home"></i>
          <span class="text-[10px] font-bold uppercase mt-1">Accueil</span>
        </button>
        <button class="flex flex-col items-center">
          <i class="fas fa-history"></i>
          <span class="text-[10px] font-bold uppercase mt-1">Histoire</span>
        </button>
        <button class="flex flex-col items-center">
          <i class="fas fa-user-md"></i>
          <span class="text-[10px] font-bold uppercase mt-1">Conseils</span>
        </button>
      </nav>

      <div class="absolute bottom-0 w-full bg-white text-[9px] text-center p-1 text-gray-400 border-t border-gray-100">
        Santé Scan est un outil d'information... Consulter un médecin.
      </div>
    </div>
  </div>
</template>