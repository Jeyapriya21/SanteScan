<template>
  <div class="w-full bg-gray-100 rounded-2xl p-5 relative mb-8 shadow-inner border border-gray-200">
    <div 
      class="absolute top-0 right-4 w-4 h-5 rounded-b-sm"
      :class="statusColor"
    ></div>
    
    <p class="text-gray-800 font-semibold text-sm leading-snug">
      {{ messagePrincipal }}<br/>
      <span class="text-gray-500 font-medium">{{ detailIA }}</span>
    </p>
  </div>
</template>

<script setup>
defineProps(['messagePrincipal', 'detailIA', 'statusColor']);
</script>