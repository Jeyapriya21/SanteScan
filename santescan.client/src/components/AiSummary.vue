<template>
  <div class="relative bg-slate-100 border-2 border-slate-200 rounded-2xl p-6 shadow-inner transition-all hover:shadow-md">
    
    <div class="absolute top-0 right-0 w-10 h-10 bg-green-500 rotate-45 translate-x-5 -translate-y-5 shadow-sm"></div>

    <div class="flex flex-col md:flex-row md:items-start gap-4">
      <div class="w-12 h-12 bg-white rounded-xl flex items-center justify-center shadow-sm shrink-0">
        <span class="text-2xl">🤖</span>
      </div>

      <div class="space-y-2">
        <h3 class="text-sm uppercase tracking-widest text-slate-500 font-bold">
          Analyse du {{ date }}
        </h3>
        
        <p class="text-xl text-slate-800 leading-relaxed">
          Votre bilan est 
          <span class="text-green-600 font-bold underline decoration-2 underline-offset-4">
            {{ status }}
          </span>.
        </p>
        
        <p class="text-lg text-slate-600 font-medium italic">
          "{{ note }}"
        </p>
      </div>
    </div>

    <div class="mt-4 pt-4 border-t border-slate-200 flex items-center gap-2">
      <span class="flex h-2 w-2 rounded-full bg-green-500"></span>
      <span class="text-[10px] text-slate-400 font-bold uppercase tracking-wider">
        Interprété par Santé Scan AI
      </span>
    </div>
  </div>
</template>

<script setup>
// On définit les propriétés pour que le composant soit réutilisable
defineProps({
  date: {
    type: String,
    default: '12/01'
  },
  status: {
    type: String,
    default: 'OK'
  },
  note: {
    type: String,
    default: 'Tout semble normal.'
  }
})
</script>