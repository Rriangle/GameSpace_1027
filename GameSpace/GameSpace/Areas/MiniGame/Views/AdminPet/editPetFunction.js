function editPet(petId) {
    // 載入寵物資料並顯示編輯模態框
    $.ajax({
        url: "@Url.Action("GetPetDetail", "AdminPet")",
        type: "GET",
        data: { petId: petId },
        success: function(response) {
            if (response.success) {
                var pet = response.data;
                document.getElementById("editPetId").value = pet.petId;
                document.getElementById("editPetName").value = pet.petName;
                document.getElementById("editPetLevel").value = pet.level;
                document.getElementById("editPetExperience").value = pet.experience;
                document.getElementById("editPetSkinColor").value = pet.skinColor;
                document.getElementById("editPetBackground").value = pet.background;
                document.getElementById("editPetStatus").value = pet.isActive;
                document.getElementById("editPetDescription").value = pet.description || "";
                
                $("#editPetModal").modal("show");
            } else {
                alert("載入寵物資料失敗: " + response.message);
            }
        },
        error: function() {
            alert("載入寵物資料失敗，請稍後再試");
        }
    });
}
