<template>
  <button 
    @click="$emit('scan')"
    class="w-full bg-blue-500 hover:bg-blue-600 transition-all shadow-lg rounded-xl p-4 flex flex-col items-center border-b-4 border-blue-700 active:border-b-0 active:mt-1 mb-4"
  >
    <div class="flex items-center space-x-3 text-white">
      <i class="fas fa-camera text-2xl"></i>
      <span class="text-xl font-bold italic tracking-wide">Scanner mon bilan</span>
    </div>
  </button>
  <p class="text-sm text-gray-400 font-medium mb-8 cursor-pointer hover:underline">
    Importer un PDF/Image
  </p>
</template>